using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using QrTransferDemo.Models;
using QrTransferDemo.Services;
using System.Runtime.Versioning;

namespace QrTransferDemo.Components;

[SupportedOSPlatform("browser")]
public partial class QrTransferReceiverTab : ComponentBase, IAsyncDisposable
{
    private const string VideoElementId = "qr-transfer-receiver-video";
    private const string CanvasElementId = "qr-transfer-receiver-canvas";
    private const string CaptureSourceScreen = "screen";
    private const string CaptureSourceCamera = "camera";

    private static readonly string[] CorrectionLevelOptions = { "L", "M", "Q", "H" };
    private static readonly IReadOnlyList<KeyValuePair<string, string>> CaptureSourceOptionList = new[]
    {
        new KeyValuePair<string, string>(CaptureSourceScreen, "Screen"),
        new KeyValuePair<string, string>(CaptureSourceCamera, "Camera")
    };

    private readonly List<ReceivedFileViewModel> _files = new();
    private readonly Dictionary<Guid, ReceivedFileViewModel> _fileIndex = new();

    private bool _domInitialized;
    private bool _isStarting;
    private bool _isCapturing;
    private bool _isPaused;
    private string? _statusMessage;
    private string? _errorMessage;
    private int _expectedChunkSize = 128;
    private int _scanInterval = 250;
    private string _correctionLevel = "M";
    private string _captureSource = CaptureSourceScreen;
    private CancellationTokenSource? _captureCts;
    private Task? _captureTask;
    private int _decodeGuard;

    private IEnumerable<ReceivedFileViewModel> OrderedFiles => _files.OrderByDescending(file => file.LastUpdated);

    private IReadOnlyList<string> CorrectionLevels => CorrectionLevelOptions;

    private IEnumerable<KeyValuePair<string, string>> CaptureSourceOptions => CaptureSourceOptionList;

    private string CaptureStatusLabel => _isCapturing ? (_isPaused ? "Paused" : "Active") : "Idle";

    private string CaptureStatusBadge => _isCapturing ? (_isPaused ? "bg-warning text-dark" : "bg-success") : "bg-secondary";

    private bool HasFiles => _files.Count > 0;

    private string CorrectionLevel
    {
        get => _correctionLevel;
        set => _correctionLevel = value;
    }

    private int ExpectedChunkSize
    {
        get => _expectedChunkSize;
        set => _expectedChunkSize = Math.Clamp(value, 16, 4096);
    }

    private int ScanInterval
    {
        get => _scanInterval;
        set => _scanInterval = Math.Clamp(value, 50, 5000);
    }

    private string CaptureSource
    {
        get => _captureSource;
        set => _captureSource = string.Equals(value, CaptureSourceCamera, StringComparison.OrdinalIgnoreCase) ? CaptureSourceCamera : CaptureSourceScreen;
    }

    [Inject]
    public required QrChunkAssembler ChunkAssembler { get; set; }

    [Inject]
    public required QrFrameDecoder FrameDecoder { get; set; }

    [Inject]
    public required BrowserMediaCapture MediaCapture { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await EnsureDomAsync();
        }
    }

    private async Task EnsureDomAsync()
    {
        if (_domInitialized)
        {
            return;
        }

        try
        {
            await MediaCapture.InitializeAsync(VideoElementId, CanvasElementId);
            _domInitialized = true;
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
            _statusMessage = null;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task StartCaptureAsync()
    {
        await EnsureDomAsync();
        if (!_domInitialized || _isStarting)
        {
            return;
        }

        if (_isCapturing)
        {
            await StopCaptureAsync();
        }

        _isStarting = true;
        _errorMessage = null;

        try
        {
            _captureCts?.Cancel();
            _captureCts?.Dispose();
            _captureCts = new CancellationTokenSource();

            var options = new BrowserMediaCapture.CaptureOptions(
                FrameRateHint: _scanInterval > 0 ? Math.Min(60d, 1000d / Math.Max(_scanInterval, 1)) : null,
                Width: null,
                Height: null,
                FacingMode: string.Equals(_captureSource, CaptureSourceCamera, StringComparison.OrdinalIgnoreCase) ? "environment" : null);

            var source = string.Equals(_captureSource, CaptureSourceCamera, StringComparison.OrdinalIgnoreCase)
                ? BrowserMediaCapture.CaptureSource.Camera
                : BrowserMediaCapture.CaptureSource.Screen;

            await MediaCapture.StartCaptureAsync(source, options, _captureCts.Token);

            _isCapturing = true;
            _isPaused = false;
            _statusMessage = "Capture started.";
            _captureTask = RunCaptureLoopAsync(_captureCts.Token);
        }
        catch (OperationCanceledException)
        {
            _statusMessage = "Capture cancelled.";
            _isCapturing = false;
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
            _statusMessage = null;
            _isCapturing = false;
            _captureCts?.Cancel();
            _captureCts?.Dispose();
            _captureCts = null;
        }
        finally
        {
            _isStarting = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task StopCaptureAsync()
    {
        _captureCts?.Cancel();

        if (_captureTask is not null)
        {
            try
            {
                await _captureTask;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _errorMessage ??= ex.Message;
                _statusMessage = null;
            }
            finally
            {
                _captureTask = null;
            }
        }

        _captureCts?.Dispose();
        _captureCts = null;

        try
        {
            await MediaCapture.StopCaptureAsync();
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
            _statusMessage = null;
        }

        _isCapturing = false;
        _isPaused = false;
        _statusMessage ??= "Capture stopped.";
        await InvokeAsync(StateHasChanged);
    }

    private Task PauseCaptureAsync()
    {
        if (!_isCapturing || _isPaused)
        {
            return Task.CompletedTask;
        }

        _isPaused = true;
        _statusMessage = "Recognition paused.";
        _errorMessage = null;
        return InvokeAsync(StateHasChanged);
    }

    private Task ResumeCaptureAsync()
    {
        if (!_isCapturing || !_isPaused)
        {
            return Task.CompletedTask;
        }

        _isPaused = false;
        _statusMessage = "Recognition resumed.";
        _errorMessage = null;
        return InvokeAsync(StateHasChanged);
    }

    private async Task RunCaptureLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_scanInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (!_isCapturing || _isPaused)
            {
                continue;
            }

            if (!MediaCapture.TryCaptureFrame(out var pixels, out var width, out var height) || pixels is null)
            {
                continue;
            }

            await ProcessFrameAsync(width, height, pixels);
        }
    }

    private Task ProcessFrameAsync(int width, int height, byte[] pixels)
    {
        if (width <= 0 || height <= 0 || pixels.Length == 0)
        {
            return Task.CompletedTask;
        }

        if (Interlocked.CompareExchange(ref _decodeGuard, 1, 0) != 0)
        {
            return Task.CompletedTask;
        }

        string? decoded = null;
        Exception? decodeError = null;

        try
        {
            if (FrameDecoder.TryDecode(pixels, width, height, out var text) && !string.IsNullOrWhiteSpace(text))
            {
                decoded = text;
            }
        }
        catch (Exception ex)
        {
            decodeError = ex;
        }
        finally
        {
            Interlocked.Exchange(ref _decodeGuard, 0);
        }

        if (decodeError is not null)
        {
            return InvokeAsync(() =>
            {
                _errorMessage = decodeError.Message;
                _statusMessage = null;
                StateHasChanged();
            });
        }

        if (string.IsNullOrWhiteSpace(decoded))
        {
            return Task.CompletedTask;
        }

        return HandleDecodedPayloadAsync(decoded);
    }

    private Task HandleDecodedPayloadAsync(string payload)
    {
        if (!TryParsePacket(payload, out var packet, out var parseError))
        {
            return InvokeAsync(() =>
            {
                _errorMessage = parseError ?? "Unable to parse frame.";
                _statusMessage = null;
                StateHasChanged();
            });
        }

        var result = ChunkAssembler.ProcessChunk(packet);
        return InvokeAsync(() =>
        {
            var chunkSizeMismatch = _expectedChunkSize > 0 && packet.ChunkSize > 0 && packet.ChunkSize != _expectedChunkSize;
            var payloadOverflow = packet.ChunkSize > 0 && packet.Payload.Length > packet.ChunkSize;
            var file = UpdateFileState(result);
            var message = result.Status switch
            {
                ChunkAcceptanceStatus.Accepted when result.File is not null => $"Completed {file.DisplayName}.",
                ChunkAcceptanceStatus.Accepted => $"Received chunk {packet.ChunkIndex + 1}/{packet.TotalChunks}.",
                ChunkAcceptanceStatus.Duplicate => "Duplicate chunk ignored.",
                ChunkAcceptanceStatus.InvalidCrc => "Chunk rejected due to invalid CRC.",
                ChunkAcceptanceStatus.InvalidMetadata => "Chunk ignored due to invalid metadata.",
                ChunkAcceptanceStatus.InvalidFileChecksum => "File checksum mismatch. Awaiting retransmission.",
                _ => _statusMessage
            };

            if (payloadOverflow)
            {
                _errorMessage = $"Chunk {packet.ChunkIndex} exceeds advertised size ({packet.Payload.Length} > {packet.ChunkSize}).";
                _statusMessage = null;
            }
            else if (chunkSizeMismatch)
            {
                _statusMessage = $"Chunk size mismatch: expected {_expectedChunkSize} bytes, received {packet.ChunkSize}.";
                _errorMessage = null;
            }
            else if (message is not null)
            {
                if (result.Status == ChunkAcceptanceStatus.InvalidCrc || result.Status == ChunkAcceptanceStatus.InvalidMetadata || result.Status == ChunkAcceptanceStatus.InvalidFileChecksum)
                {
                    _errorMessage = message;
                    _statusMessage = null;
                }
                else
                {
                    _statusMessage = message;
                    _errorMessage = null;
                }
            }

            StateHasChanged();
        });
    }

    private Task ResetFileAsync(ReceivedFileViewModel file)
    {
        if (ChunkAssembler.Reset(file.FileId))
        {
            file.ResetProgress();
            _statusMessage = $"Reset {file.DisplayName}.";
            _errorMessage = null;
        }

        return InvokeAsync(StateHasChanged);
    }

    private Task RemoveFileAsync(ReceivedFileViewModel file)
    {
        ChunkAssembler.Remove(file.FileId);
        _fileIndex.Remove(file.FileId);
        _files.Remove(file);
        _statusMessage = $"Cleared {file.DisplayName}.";
        _errorMessage = null;
        return InvokeAsync(StateHasChanged);
    }

    private Task ClearAllAsync()
    {
        foreach (var file in _files.ToList())
        {
            ChunkAssembler.Remove(file.FileId);
        }

        _files.Clear();
        _fileIndex.Clear();
        _statusMessage = "Cleared all files.";
        _errorMessage = null;
        return InvokeAsync(StateHasChanged);
    }

    private ReceivedFileViewModel UpdateFileState(ChunkProcessingResult result)
    {
        if (!_fileIndex.TryGetValue(result.Snapshot.FileId, out var viewModel))
        {
            viewModel = new ReceivedFileViewModel(result.Snapshot.FileId);
            _fileIndex[result.Snapshot.FileId] = viewModel;
            _files.Add(viewModel);
        }

        viewModel.Apply(result.Snapshot, result.File);
        return viewModel;
    }

    private static bool TryParsePacket(string payload, out QrChunkPacket packet, out string? error)
    {
        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            if (!root.TryGetProperty("fid", out var fileIdProperty))
            {
                throw new InvalidOperationException("Missing file identifier.");
            }

            var type = root.TryGetProperty("t", out var typeProperty) ? typeProperty.GetString() ?? string.Empty : string.Empty;
            if (!string.IsNullOrEmpty(type) && !string.Equals(type, "chunk", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Unsupported frame type '{type}'.");
            }

            var fileId = fileIdProperty.GetGuid();
            var fileName = root.TryGetProperty("name", out var nameProperty) ? nameProperty.GetString() ?? string.Empty : string.Empty;
            var fileSize = root.TryGetProperty("fs", out var sizeProperty) ? sizeProperty.GetInt64() : 0L;
            var chunkSize = root.TryGetProperty("cs", out var chunkSizeProperty) ? chunkSizeProperty.GetInt32() : 0;
            var correctionLevel = root.TryGetProperty("ecc", out var eccProperty) ? eccProperty.GetString() ?? string.Empty : string.Empty;
            var chunkIndex = root.TryGetProperty("ci", out var chunkIndexProperty) ? chunkIndexProperty.GetInt32() : 0;
            var totalChunks = root.TryGetProperty("tc", out var totalChunksProperty) ? totalChunksProperty.GetInt32() : 0;
            var base64 = root.TryGetProperty("p", out var payloadProperty) ? payloadProperty.GetString() ?? string.Empty : string.Empty;
            var payloadCrc = root.TryGetProperty("crc", out var crcProperty) ? crcProperty.GetUInt32() : 0u;
            var fileCrc = root.TryGetProperty("fcrc", out var fileCrcProperty) ? fileCrcProperty.GetUInt32() : 0u;

            if (fileSize > 0 && !root.TryGetProperty("fcrc", out _))
            {
                throw new InvalidOperationException("Missing file checksum.");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new InvalidOperationException("Missing file name.");
            }

            var payloadBytes = string.IsNullOrEmpty(base64) ? Array.Empty<byte>() : Convert.FromBase64String(base64);
            packet = new QrChunkPacket(fileId, fileName, fileSize, chunkSize, correctionLevel, chunkIndex, totalChunks, payloadBytes, payloadCrc, fileCrc);
            error = null;
            return true;
        }
        catch (Exception ex)
        {
            packet = default!;
            error = ex.Message;
            return false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            _captureCts?.Cancel();
            if (_captureTask is not null)
            {
                try
                {
                    await _captureTask;
                }
                catch
                {
                }
            }

            await MediaCapture.StopCaptureAsync();
            await MediaCapture.DisposeAsync();
        }
        catch
        {
        }
        finally
        {
            _captureTask = null;
            _captureCts?.Dispose();
            _captureCts = null;
        }
    }

    private sealed class ReceivedFileViewModel
    {
        private IReadOnlyList<int> _missingChunks = Array.Empty<int>();

        public ReceivedFileViewModel(Guid fileId)
        {
            FileId = fileId;
        }

        public Guid FileId { get; }

        public string FileName { get; private set; } = string.Empty;

        public long FileSize { get; private set; }

        public int ChunkSize { get; private set; }

        public string CorrectionLevel { get; private set; } = string.Empty;

        public int TotalChunks { get; private set; }

        public int ReceivedChunks { get; private set; }

        public long ReceivedBytes { get; private set; }

        public IReadOnlyList<int> MissingChunks => _missingChunks;

        public int InvalidChunks { get; private set; }

        public int ChecksumFailures { get; private set; }

        public DateTimeOffset LastUpdated { get; private set; }

        public uint FileChecksum { get; private set; }

        public byte[]? Data { get; private set; }

        public string? DownloadUrl { get; private set; }

        public bool IsCompleted => DownloadUrl is not null;

        public bool IsWaiting => !IsCompleted && (MissingChunks.Count > 0 || ChecksumFailures > 0);

        public int MissingCount => _missingChunks.Count;

        public int ProgressPercent => TotalChunks == 0 ? 0 : (int)Math.Round(ReceivedChunks * 100d / TotalChunks);

        public string DisplayName => string.IsNullOrWhiteSpace(FileName) ? $"File {FileId.ToString()[..8]}" : FileName;

        public string SuggestedFileName => string.IsNullOrWhiteSpace(FileName) ? $"qr-{FileId.ToString()[..8]}.bin" : FileName;

        public string Configuration
        {
            get
            {
                var chunkDisplay = ChunkSize > 0 ? $"{ChunkSize} B" : "—";
                var eccDisplay = string.IsNullOrWhiteSpace(CorrectionLevel) ? "—" : CorrectionLevel;
                return $"{chunkDisplay} · {eccDisplay}";
            }
        }

        public string MissingPreview
        {
            get
            {
                if (_missingChunks.Count == 0)
                {
                    return string.Empty;
                }

                var ordered = _missingChunks.OrderBy(index => index).ToArray();
                if (ordered.Length <= 8)
                {
                    return string.Join(", ", ordered);
                }

                return string.Join(", ", ordered.Take(8)) + "…";
            }
        }

        public string ChecksumHex => FileChecksum.ToString("X8");

        public void Apply(FileAssemblySnapshot snapshot, AssembledFile? file)
        {
            FileName = snapshot.FileName;
            FileSize = snapshot.FileSize;
            ChunkSize = snapshot.ChunkSize;
            CorrectionLevel = snapshot.CorrectionLevel;
            TotalChunks = snapshot.TotalChunks;
            ReceivedChunks = snapshot.ReceivedChunks;
            ReceivedBytes = snapshot.ReceivedBytes;
            _missingChunks = snapshot.MissingChunks.ToArray();
            InvalidChunks = snapshot.InvalidChunks;
            ChecksumFailures = snapshot.ChecksumFailures;
            FileChecksum = snapshot.FileChecksum;
            LastUpdated = snapshot.LastUpdated;
            if (file is not null)
            {
                Data = file.Data.ToArray();
                DownloadUrl = CreateDownloadUrl(Data);
            }
            else if (!snapshot.IsCompleted)
            {
                Data = null;
                DownloadUrl = null;
            }
        }

        public void ResetProgress()
        {
            Data = null;
            DownloadUrl = null;
            ReceivedChunks = 0;
            ReceivedBytes = 0;
            InvalidChunks = 0;
            ChecksumFailures = 0;
            _missingChunks = Array.Empty<int>();
            ChunkSize = 0;
            CorrectionLevel = string.Empty;
            LastUpdated = DateTimeOffset.UtcNow;
        }

        private static string CreateDownloadUrl(byte[] data)
        {
            if (data.Length == 0)
            {
                return "data:application/octet-stream;base64,";
            }

            var base64 = Convert.ToBase64String(data);
            return $"data:application/octet-stream;base64,{base64}";
        }
    }
}
