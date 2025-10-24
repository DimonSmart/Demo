using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Net.Codecrete.QrCodeGenerator;
using QrTransferDemo.Models;
using QrTransferDemo.Services;
using QrTransferDemo.Utilities;

namespace QrTransferDemo.Components;

public partial class QrTransferSenderTab : ComponentBase, IAsyncDisposable
{
    private const long MaxFileSize = 1024L * 1024L * 128L;

    private static readonly IReadOnlyList<int> PixelSizes = new List<int> { 256, 320, 384, 448, 512 };
    private static readonly IReadOnlyList<string> CorrectionLevels = new List<string> { "L", "M", "Q", "H" };

    private readonly List<QueuedFile> _queue = new();
    private readonly CancellationTokenSource _lifetimeCts = new();

    private bool _isDragging;
    private int _dragCounter;
    private string? _currentQrMarkup;

    private int _selectedQrVersion = 5;
    private int _qrPixelSize = 320;
    private int _frameRate = 2;
    private string _correctionLevel = "M";
    private int _chunkSize = 128;
    private string? _validationMessage;

    private bool _isRunning;
    private bool _repeat;
    private bool _loopStarted;
    private bool _canRestart;
    private int _currentFileIndex;
    private QrChunkBuilder? _chunkBuilder;
    private QrCapacityCatalog? _capacityCatalog;

    private QueuedFile? CurrentFile => _queue.Count > 0 && _currentFileIndex < _queue.Count ? _queue[_currentFileIndex] : null;
    private QueuedFile? NextFile => ResolveNextFile();

    [Inject]
    public required IServiceProvider Services { get; set; }

    private QrChunkBuilder ChunkBuilder => _chunkBuilder ??= Services.GetService<QrChunkBuilder>() ?? new QrChunkBuilder();
    private QrCapacityCatalog CapacityCatalog => _capacityCatalog ??= Services.GetService<QrCapacityCatalog>() ?? new QrCapacityCatalog();

    public ValueTask DisposeAsync()
    {
        _lifetimeCts.Cancel();
        _lifetimeCts.Dispose();
        return ValueTask.CompletedTask;
    }

    private async Task HandleFilesSelectedAsync(InputFileChangeEventArgs args)
    {
        if (args.FileCount == 0)
        {
            return;
        }

        foreach (var file in args.GetMultipleFiles())
        {
            if (file.Size > MaxFileSize)
            {
                _validationMessage = $"{file.Name} is larger than {FileSizeFormatter.Format(MaxFileSize)}.";
                StateHasChanged();
                continue;
            }

            await using var stream = file.OpenReadStream(MaxFileSize);
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory, _lifetimeCts.Token);
            var data = memory.ToArray();

            var queuedFile = new QueuedFile(file.Name, file.Size, data)
            {
                Packets = ChunkBuilder.BuildPackets(file.Name, data, _chunkSize, _correctionLevel)
            };
            _queue.Add(queuedFile);
        }

        _validationMessage = ValidateChunkSize();
        ResetTransmissionState();
        StateHasChanged();
    }

    private IReadOnlyList<int> SupportedVersions => CapacityCatalog.SupportedVersions;

    private IReadOnlyList<int> AvailablePixelSizes => PixelSizes;

    private IReadOnlyList<string> AvailableCorrectionLevels => CorrectionLevels;

    private int SelectedQrVersion
    {
        get => _selectedQrVersion;
        set
        {
            if (_selectedQrVersion == value)
            {
                return;
            }

            _selectedQrVersion = value;
            _validationMessage = ValidateChunkSize();
        }
    }

    private int QrPixelSize
    {
        get => _qrPixelSize;
        set => _qrPixelSize = value;
    }

    private int FrameRate
    {
        get => _frameRate;
        set => _frameRate = Math.Clamp(value, 1, 30);
    }

    private string CorrectionLevel
    {
        get => _correctionLevel;
        set
        {
            if (string.Equals(_correctionLevel, value, StringComparison.Ordinal))
            {
                return;
            }

            _correctionLevel = value;
            _validationMessage = ValidateChunkSize();
        }
    }

    private int ChunkSize
    {
        get => _chunkSize;
        set
        {
            if (_chunkSize == value)
            {
                return;
            }

            _chunkSize = value;
            _validationMessage = ValidateChunkSize();
            if (_validationMessage is null)
            {
                RebuildPackets();
            }
        }
    }

    private string? ValidateChunkSize()
    {
        if (_chunkSize <= 0)
        {
            return "Chunk size must be greater than zero.";
        }

        if (!CapacityCatalog.TryGetCapacity(_selectedQrVersion, _correctionLevel, out var capacity))
        {
            return "Unsupported QR configuration.";
        }

        if (_chunkSize > capacity)
        {
            return $"Chunk size exceeds QR capacity ({capacity} bytes) for version {_selectedQrVersion} with {_correctionLevel} correction.";
        }

        return null;
    }

    private void RebuildPackets()
    {
        foreach (var file in _queue)
        {
            file.Packets = ChunkBuilder.BuildPackets(file.Name, file.Data, _chunkSize, _correctionLevel);
            file.Reset();
        }
        ResetTransmissionState();
    }

    private bool StartDisabled => _queue.Count == 0 || _validationMessage is not null;

    private Task StartAsync()
    {
        if (StartDisabled)
        {
            return Task.CompletedTask;
        }

        _isRunning = true;
        _canRestart = true;

        if (!_loopStarted)
        {
            _loopStarted = true;
            _ = RunTransmissionLoopAsync(_lifetimeCts.Token);
        }

        return Task.CompletedTask;
    }

    private void Pause()
    {
        _isRunning = false;
    }

    private async Task RestartAsync()
    {
        if (!_canRestart)
        {
            return;
        }

        Pause();
        foreach (var file in _queue)
        {
            file.Reset();
        }
        _currentFileIndex = 0;
        await RenderEmptyAsync();
        StateHasChanged();
    }

    private async Task RunTransmissionLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                if (!_isRunning)
                {
                    await Task.Delay(100, token);
                    continue;
                }

                if (_queue.Count == 0)
                {
                    _isRunning = false;
                    await InvokeAsync(StateHasChanged);
                    continue;
                }

                var file = CurrentFile;
                if (file is null)
                {
                    _isRunning = false;
                    await InvokeAsync(StateHasChanged);
                    continue;
                }

                if (file.Packets is null || file.Packets.Count == 0)
                {
                    MoveToNextFile();
                    continue;
                }

                if (file.IsCompleted)
                {
                    if (!MoveToNextFile())
                    {
                        _isRunning = false;
                        await InvokeAsync(StateHasChanged);
                        continue;
                    }

                    continue;
                }

                var packet = file.Packets[file.NextChunkIndex];
                file.NextChunkIndex++;
                if (file.NextChunkIndex >= file.Packets.Count)
                {
                    file.MarkCompleted();
                }

                await RenderPacketAsync(packet);
                await InvokeAsync(StateHasChanged);

                var delay = TimeSpan.FromMilliseconds(Math.Max(1, 1000 / _frameRate));
                await Task.Delay(delay, token);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private bool MoveToNextFile()
    {
        var nextIndex = FindNextIndex();
        if (nextIndex < 0)
        {
            if (_repeat)
            {
                foreach (var file in _queue)
                {
                    file.Reset();
                }
                _currentFileIndex = 0;
                return _queue.Count > 0;
            }

            return false;
        }

        _currentFileIndex = nextIndex;
        return true;
    }

    private int FindNextIndex()
    {
        if (_queue.Count == 0)
        {
            return -1;
        }

        for (var index = _currentFileIndex + 1; index < _queue.Count; index++)
        {
            if (!_queue[index].IsCompleted)
            {
                return index;
            }
        }

        for (var index = 0; index <= _currentFileIndex; index++)
        {
            if (!_queue[index].IsCompleted)
            {
                return index;
            }
        }

        return -1;
    }

    private QueuedFile? ResolveNextFile()
    {
        if (_queue.Count == 0)
        {
            return null;
        }

        var index = FindNextIndex();
        if (index < 0 || index == _currentFileIndex)
        {
            return null;
        }

        return _queue[index];
    }

    private Task RenderPacketAsync(QrChunkPacket packet)
    {
        var payload = SerializePacket(packet);
        _currentQrMarkup = CreateQrMarkup(payload);
        return Task.CompletedTask;
    }

    private Task RenderEmptyAsync()
    {
        _currentQrMarkup = null;
        return Task.CompletedTask;
    }

    private string CreateQrMarkup(string payload)
    {
        var ecc = CorrectionLevel switch
        {
            "L" => QrCode.Ecc.Low,
            "M" => QrCode.Ecc.Medium,
            "Q" => QrCode.Ecc.Quartile,
            "H" => QrCode.Ecc.High,
            _ => QrCode.Ecc.Medium
        };

        var data = Encoding.UTF8.GetBytes(payload);
        var segments = new List<QrSegment> { QrSegment.MakeBytes(data) };
        var qr = QrCode.EncodeSegments(segments, ecc, _selectedQrVersion, _selectedQrVersion, -1, false);
        var svg = qr.ToSvgString(0);

        const string svgTag = "<svg ";
        if (svg.StartsWith(svgTag, StringComparison.Ordinal))
        {
            svg = svgTag + "width=\"100%\" height=\"100%\" " + svg.Substring(svgTag.Length);
        }

        return $"<div style=\"width:{_qrPixelSize}px;height:{_qrPixelSize}px\">{svg}</div>";
    }

    private static string SerializePacket(QrChunkPacket packet)
    {
        var payload = Convert.ToBase64String(packet.Payload);
        var dto = new
        {
            t = "chunk",
            fid = packet.FileId,
            name = packet.FileName,
            fs = packet.FileSize,
            cs = packet.ChunkSize,
            ecc = packet.CorrectionLevel,
            ci = packet.ChunkIndex,
            tc = packet.TotalChunks,
            p = payload,
            crc = packet.PayloadCrc32,
            fcrc = packet.FileCrc32
        };
        return JsonSerializer.Serialize(dto);
    }

    private void ResetTransmissionState()
    {
        _currentFileIndex = 0;
        foreach (var file in _queue)
        {
            file.Reset();
        }
        _canRestart = _queue.Count > 0;
        _currentQrMarkup = null;
    }

    private void OnDragEnter(DragEventArgs _)
    {
        _dragCounter++;
        if (!_isDragging)
        {
            _isDragging = true;
            StateHasChanged();
        }
    }

    private void OnDragLeave(DragEventArgs _)
    {
        if (_dragCounter > 0)
        {
            _dragCounter--;
        }

        if (_dragCounter == 0 && _isDragging)
        {
            _isDragging = false;
            StateHasChanged();
        }
    }

    private void OnDragOver(DragEventArgs _)
    {
    }

    private void OnDrop(DragEventArgs _)
    {
        _dragCounter = 0;
        if (_isDragging)
        {
            _isDragging = false;
            StateHasChanged();
        }
    }

    private sealed class QueuedFile
    {
        public QueuedFile(string name, long size, byte[] data)
        {
            Name = name;
            Size = size;
            Data = data;
        }

        public string Name { get; }
        public long Size { get; }
        public byte[] Data { get; }
        public IReadOnlyList<QrChunkPacket>? Packets { get; set; }
        public int NextChunkIndex { get; set; }
        public bool IsCompleted { get; private set; }

        public void Reset()
        {
            NextChunkIndex = 0;
            IsCompleted = false;
        }

        public void MarkCompleted()
        {
            IsCompleted = true;
        }
    }
}
