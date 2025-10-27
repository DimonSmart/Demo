using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Net.Codecrete.QrCodeGenerator;
using QrTransferDemo.Models;
using QrTransferDemo.Services;
using QrTransferDemo.Utilities;

namespace QrTransferDemo.Components;

public partial class QrTransferSenderTab : ComponentBase, IAsyncDisposable
{
    private const int MaxQueuedFiles = 16;
    private const int FrameHeaderLength = 6;
    private const int MinFrameDuration = 50;
    private const int MaxFrameDuration = 5000;
    private const int DefaultFrameDuration = 250;
    private const int MaxPayloadLength = byte.MaxValue;
    private const int MaxFileSize = ushort.MaxValue;

    private static readonly IReadOnlyList<CorrectionLevelOption> CorrectionLevels = new List<CorrectionLevelOption>
    {
        new("L", "L · 7% redundancy (max data)", "Keeps roughly 93% of the codewords for payload."),
        new("M", "M · 15% redundancy", "Balances capacity and resilience with ~15% dedicated to error correction."),
        new("Q", "Q · 25% redundancy", "Uses about a quarter of the codewords for correction."),
        new("H", "H · 30% redundancy (max protection)", "Provides the strongest correction with the lowest payload capacity.")
    };

    private readonly List<QueuedFile> _queue = new();
    private readonly CancellationTokenSource _lifetimeCts = new();

    private bool _isDragging;
    private int _dragCounter;
    private string? _currentQrMarkup;

    private int _selectedQrVersion = 10;
    private int _frameDuration = DefaultFrameDuration;
    private string _correctionLevel = "L";
    private int _chunkSize;
    private string? _validationMessage;

    private bool _isRunning;
    private bool _loopStarted;
    private bool _canRestart;
    private int _currentFileIndex;
    private QrChunkBuilder? _chunkBuilder;
    private QrCapacityCatalog? _capacityCatalog;
    private bool _isFullscreen;
    private string? _transmissionError;

    private QueuedFile? CurrentFile => _queue.Count > 0 && _currentFileIndex < _queue.Count ? _queue[_currentFileIndex] : null;
    private QueuedFile? NextFile => ResolveNextFile();

    [Inject]
    public required IServiceProvider Services { get; set; }

    [Inject]
    public required ILogger<QrTransferSenderTab> Logger { get; set; }

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

        _validationMessage = null;

        foreach (var file in args.GetMultipleFiles())
        {
            if (_queue.Count >= MaxQueuedFiles)
            {
                _validationMessage = $"Only {MaxQueuedFiles} files can be queued at once.";
                StateHasChanged();
                break;
            }

            if (_chunkSize <= 0)
            {
                _validationMessage = "Selected QR configuration cannot encode payloads.";
                StateHasChanged();
                break;
            }

            if (file.Size > MaxFileSize)
            {
                _validationMessage = $"{file.Name} is larger than {FileSizeFormatter.Format(MaxFileSize)}.";
                StateHasChanged();
                continue;
            }

            var nameBytes = Encoding.UTF8.GetByteCount(file.Name);
            if (nameBytes > byte.MaxValue)
            {
                _validationMessage = $"{file.Name} has a UTF-8 name longer than 255 bytes.";
                StateHasChanged();
                continue;
            }

            await using var stream = file.OpenReadStream(MaxFileSize);
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory, _lifetimeCts.Token);
            var data = memory.ToArray();

            var fileIndex = (byte)_queue.Count;
            var queuedFile = new QueuedFile(fileIndex, file.Name, file.Size, data)
            {
                Packets = ChunkBuilder.BuildPackets(fileIndex, file.Name, data, _chunkSize, _correctionLevel)
            };
            _queue.Add(queuedFile);
        }

        if (_validationMessage is null)
        {
            UpdateChunkSize();
        }
        ResetTransmissionState();
        StateHasChanged();
    }

    private IReadOnlyList<int> SupportedVersions => CapacityCatalog.SupportedVersions;

    private IReadOnlyList<CorrectionLevelOption> AvailableCorrectionLevels => CorrectionLevels;

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
            UpdateChunkSize(forceRebuild: true);
        }
    }

    private int FrameDuration
    {
        get => _frameDuration;
        set => _frameDuration = Math.Clamp(value, MinFrameDuration, MaxFrameDuration);
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
            UpdateChunkSize(forceRebuild: true);
        }
    }

    private int ChunkSize => _chunkSize;

    protected override void OnInitialized()
    {
        UpdateChunkSize();
        base.OnInitialized();
    }

    private void UpdateChunkSize(bool forceRebuild = false)
    {
        if (!CapacityCatalog.TryGetCapacity(_selectedQrVersion, _correctionLevel, out var capacity))
        {
            _chunkSize = 0;
            _validationMessage = "Unsupported QR configuration.";
            return;
        }

        var recommendedChunkSize = CalculateEffectiveChunkSize(capacity);
        if (recommendedChunkSize <= 0)
        {
            _chunkSize = 0;
            _validationMessage = "Selected QR configuration cannot fit frame metadata.";
            Logger.LogError("Unable to calculate chunk size for capacity {Capacity}.", capacity);
            return;
        }

        _validationMessage = null;
        var chunkChanged = _chunkSize != recommendedChunkSize;
        _chunkSize = recommendedChunkSize;

        if (chunkChanged)
        {
            Logger.LogInformation("Chunk size adjusted to {ChunkSize} for capacity {Capacity}.", _chunkSize, capacity);
        }

        if (_queue.Count > 0 && (chunkChanged || forceRebuild))
        {
            RebuildPackets();
        }
    }

    private static int CalculateEffectiveChunkSize(int qrCapacity)
    {
        if (qrCapacity <= FrameHeaderLength)
        {
            return 0;
        }

        var usable = qrCapacity - FrameHeaderLength;
        return Math.Min(usable, MaxPayloadLength);
    }

    private void RebuildPackets()
    {
        foreach (var file in _queue)
        {
            file.Packets = ChunkBuilder.BuildPackets(file.FileIndex, file.Name, file.Data, _chunkSize, _correctionLevel);
            file.Reset();
        }
        ResetTransmissionState();
    }

    private bool StartDisabled => _queue.Count == 0 || _validationMessage is not null;

    private Task StartAsync()
    {
        if (StartDisabled)
        {
            Logger.LogWarning("Start requested while disabled. Queue count: {QueueCount}. Validation: {ValidationMessage}", _queue.Count, _validationMessage);
            return Task.CompletedTask;
        }

        _transmissionError = null;
        _isRunning = true;
        _canRestart = true;

        if (_queue.Count > 0)
        {
            EnterFullscreen();
        }

        Logger.LogInformation("Transmission started. Files: {FileCount}, Payload size: {ChunkSize}, Frame duration: {FrameDuration} ms.", _queue.Count, _chunkSize, _frameDuration);

        if (!_loopStarted)
        {
            _loopStarted = true;
            _ = RunTransmissionLoopAsync(_lifetimeCts.Token);
        }

        StateHasChanged();
        return Task.CompletedTask;
    }

    private void Pause()
    {
        if (!_isRunning)
        {
            return;
        }

        _isRunning = false;
        ExitFullscreen();
        Logger.LogInformation("Transmission paused at file index {Index}.", _currentFileIndex);
        StateHasChanged();
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
        _transmissionError = null;
        Logger.LogInformation("Transmission restarted.");
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
                    await StopTransmissionAsync("Queue is empty.");
                    continue;
                }

                var file = CurrentFile;
                if (file is null)
                {
                    await StopTransmissionAsync("Current file is unavailable.");
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
                        await StopTransmissionAsync("Transmission completed.");
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

                try
                {
                    await RenderPacketAsync(packet);
                    await InvokeAsync(StateHasChanged);
                }
                catch (Exception ex)
                {
                    var message = $"Failed to render frame: {ex.Message}";
                    await StopTransmissionWithErrorAsync(message, ex);
                    continue;
                }

                var delay = TimeSpan.FromMilliseconds(_frameDuration);
                await Task.Delay(delay, token);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            var message = $"Transmission loop failed: {ex.Message}";
            await StopTransmissionWithErrorAsync(message, ex);
        }
    }

    private bool MoveToNextFile()
    {
        var nextIndex = FindNextIndex();
        if (nextIndex >= 0)
        {
            _currentFileIndex = nextIndex;
            return true;
        }

        if (_queue.Count == 0)
        {
            return false;
        }

        foreach (var file in _queue)
        {
            file.Reset();
        }

        _currentFileIndex = 0;
        return _queue.Count > 0;
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

    private string CreateQrMarkup(byte[] payload)
    {
        var ecc = CorrectionLevel switch
        {
            "L" => QrCode.Ecc.Low,
            "M" => QrCode.Ecc.Medium,
            "Q" => QrCode.Ecc.Quartile,
            "H" => QrCode.Ecc.High,
            _ => QrCode.Ecc.Medium
        };

        var segments = new List<QrSegment> { QrSegment.MakeBytes(payload) };
        var qr = QrCode.EncodeSegments(segments, ecc, _selectedQrVersion, _selectedQrVersion, -1, false);
        var svg = qr.ToSvgString(0);

        const string svgTag = "<svg ";
        if (svg.StartsWith(svgTag, StringComparison.Ordinal))
        {
            svg = svgTag + "width=\"100%\" height=\"100%\" " + svg.Substring(svgTag.Length);
        }

        return $"<div class=\"qr-frame\">{svg}</div>";
    }

    private static byte[] SerializePacket(QrChunkPacket packet)
    {
        var buffer = new byte[FrameHeaderLength + packet.Payload.Length];
        var flags = (byte)(packet.FileIndex & 0x0F);
        if (packet.IsMetadata)
        {
            flags |= 0x80;
        }

        buffer[0] = flags;
        buffer[1] = (byte)packet.Payload.Length;
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(2, 2), packet.TotalLength);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(4, 2), packet.Offset);
        packet.Payload.CopyTo(buffer.AsSpan(FrameHeaderLength));
        return buffer;
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
        _isRunning = false;
        _transmissionError = null;
        ExitFullscreen();
    }

    private void EnterFullscreen()
    {
        _isFullscreen = true;
    }

    private void ExitFullscreen()
    {
        _isFullscreen = false;
    }

    private Task StopTransmissionAsync(string reason)
    {
        Logger.LogInformation("Transmission stopped: {Reason}", reason);
        return InvokeAsync(() =>
        {
            _isRunning = false;
            ExitFullscreen();
            StateHasChanged();
        });
    }

    private Task StopTransmissionWithErrorAsync(string message, Exception exception)
    {
        Logger.LogError(exception, "Transmission failed: {Message}", message);
        return InvokeAsync(() =>
        {
            _isRunning = false;
            _transmissionError = message;
            ExitFullscreen();
            StateHasChanged();
        });
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

    private sealed record CorrectionLevelOption(string Code, string Label, string Hint);

    private sealed class QueuedFile
    {
        public QueuedFile(byte fileIndex, string name, long size, byte[] data)
        {
            FileIndex = fileIndex;
            Name = name;
            Size = size;
            Data = data;
        }

        public byte FileIndex { get; }
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
