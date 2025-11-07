using System;
using System.Runtime.Versioning;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using KristofferStrube.Blazor.MediaCaptureStreams;
using Microsoft.JSInterop;

namespace QrTransferDemo.Services;

[SupportedOSPlatform("browser")]
public sealed class BrowserMediaCapture : IAsyncDisposable
{
    private const string ModuleName = "./_content/QrTransferDemo/browserMediaCapture.js";

    private readonly IJSRuntime _jsRuntime;
    private readonly IMediaDevicesService _mediaDevicesService;
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    private MediaDevices? _mediaDevices;
    private MediaStream? _mediaStream;
    private int? _contextId;
    private int _lastFrameVersion;

    public BrowserMediaCapture(IJSRuntime jsRuntime, IMediaDevicesService mediaDevicesService)
    {
        _jsRuntime = jsRuntime;
        _mediaDevicesService = mediaDevicesService;
        _moduleTask = new(() => _jsRuntime.InvokeAsync<IJSObjectReference>("import", ModuleName).AsTask());
    }

    public async Task InitializeAsync(string videoElementId, string canvasElementId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_contextId.HasValue)
        {
            return;
        }

        var module = await _moduleTask.Value.ConfigureAwait(false);

        await Task.Yield();

        _contextId = await module.InvokeAsync<int>("createContext", videoElementId, canvasElementId).ConfigureAwait(false);
        _lastFrameVersion = 0;
    }

    public async Task<bool> IsScreenCaptureSupportedAsync(CancellationToken cancellationToken = default)
    {
        var module = await _moduleTask.Value.ConfigureAwait(false);
        return await module.InvokeAsync<bool>("isScreenCaptureSupported", cancellationToken).ConfigureAwait(false);
    }

    public async Task StartCaptureAsync(CaptureSource source, CaptureOptions options, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_contextId is null)
        {
            throw new InvalidOperationException("Capture has not been initialized.");
        }

        await StopCaptureAsync().ConfigureAwait(false);

        _mediaDevices ??= await _mediaDevicesService.GetMediaDevicesAsync().ConfigureAwait(false);

        MediaStream stream = source switch
        {
            CaptureSource.Screen => await StartScreenCaptureAsync(options, cancellationToken).ConfigureAwait(false),
            CaptureSource.Camera => await StartCameraCaptureAsync(options, cancellationToken).ConfigureAwait(false),
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };

        var module = await _moduleTask.Value.ConfigureAwait(false);
        await module.InvokeVoidAsync("attachStream", cancellationToken, _contextId.Value, stream.JSReference).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        _mediaStream = stream;
        _lastFrameVersion = 0;
    }

    public async Task StopCaptureAsync()
    {
        if (_contextId is null)
        {
            await StopStreamAsync().ConfigureAwait(false);
            return;
        }

        var module = await _moduleTask.Value.ConfigureAwait(false);
        await module.InvokeVoidAsync("stopCapture", _contextId.Value).ConfigureAwait(false);
        await StopStreamAsync().ConfigureAwait(false);
        _lastFrameVersion = 0;
    }

    public async ValueTask<FrameData?> TryCaptureFrameAsync(CancellationToken cancellationToken = default)
    {
        if (_contextId is null)
        {
            return null;
        }

        var module = await _moduleTask.Value.ConfigureAwait(false);
        var frame = await module.InvokeAsync<FrameData?>("tryCaptureFrame", cancellationToken, _contextId.Value, _lastFrameVersion).ConfigureAwait(false);

        if (frame is null)
        {
            return null;
        }

        if (frame.Version == 0 || frame.Version == _lastFrameVersion)
        {
            return null;
        }

        if (frame.Width <= 0 || frame.Height <= 0)
        {
            return null;
        }

        if (frame.Pixels is null || frame.Pixels.Length == 0)
        {
            return null;
        }

        _lastFrameVersion = frame.Version;
        return frame;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await StopCaptureAsync().ConfigureAwait(false);
        }
        catch
        {
        }

        if (_contextId is int contextId)
        {
            try
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                await module.InvokeVoidAsync("disposeContext", contextId).ConfigureAwait(false);
            }
            catch
            {
            }
            finally
            {
                _contextId = null;
                _lastFrameVersion = 0;
            }
        }

        if (_mediaStream is not null)
        {
            await _mediaStream.DisposeAsync().ConfigureAwait(false);
            _mediaStream = null;
        }

        if (_mediaDevices is not null)
        {
            await _mediaDevices.DisposeAsync().ConfigureAwait(false);
            _mediaDevices = null;
        }

        if (_moduleTask.IsValueCreated)
        {
            try
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                await module.DisposeAsync().ConfigureAwait(false);
            }
            catch
            {
            }
        }
    }

    public readonly record struct CaptureOptions(double? FrameRateHint, int? Width, int? Height, string? FacingMode);

    public enum CaptureSource
    {
        Screen = 0,
        Camera = 1
    }

    private async Task<MediaStream> StartScreenCaptureAsync(CaptureOptions options, CancellationToken cancellationToken)
    {
        if (_mediaDevices is null)
        {
            throw new InvalidOperationException("Media devices are not available.");
        }

        var module = await _moduleTask.Value.ConfigureAwait(false);
        var isScreenCaptureSupported = await module.InvokeAsync<bool>("isScreenCaptureSupported", cancellationToken).ConfigureAwait(false);

        if (!isScreenCaptureSupported)
        {
            throw new NotSupportedException("Screen capture is not supported on this device.");
        }

        var displayOptions = new DisplayMediaStreamOptions
        {
            Audio = false,
            Video = new DisplayMediaVideoOptions
            {
                FrameRate = options.FrameRateHint,
                Width = options.Width,
                Height = options.Height
            }
        };

        var jsStream = await _mediaDevices.JSReference.InvokeAsync<IJSObjectReference>("getDisplayMedia", cancellationToken, displayOptions).ConfigureAwait(false);
        return await MediaStream.CreateAsync(_jsRuntime, jsStream).ConfigureAwait(false);
    }

    private async Task<MediaStream> StartCameraCaptureAsync(CaptureOptions options, CancellationToken cancellationToken)
    {
        if (_mediaDevices is null)
        {
            throw new InvalidOperationException("Media devices are not available.");
        }

        var videoConstraints = new MediaTrackConstraints();

        if (options.FrameRateHint is { } frameRate && frameRate > 0)
        {
            videoConstraints.FrameRate = new ConstrainDoubleRange { Ideal = frameRate };
        }

        if (options.Width is { } width && width > 0)
        {
            videoConstraints.Width = new ConstrainULongRange { Ideal = (ulong)width };
        }

        if (options.Height is { } height && height > 0)
        {
            videoConstraints.Height = new ConstrainULongRange { Ideal = (ulong)height };
        }

        if (!string.IsNullOrWhiteSpace(options.FacingMode) && TryParseFacingMode(options.FacingMode!, out var facingMode))
        {
            videoConstraints.FacingMode = new ConstrainVideoFacingModeParameters { Ideal = facingMode };
        }

        var constraints = new MediaStreamConstraints
        {
            Audio = false,
            Video = videoConstraints
        };

        return await _mediaDevices.GetUserMediaAsync(constraints).ConfigureAwait(false);
    }

    private async Task StopStreamAsync()
    {
        if (_mediaStream is null)
        {
            return;
        }

        try
        {
            var tracks = await _mediaStream.GetTracksAsync().ConfigureAwait(false);
            foreach (var track in tracks)
            {
                try
                {
                    await track.StopAsync().ConfigureAwait(false);
                }
                catch
                {
                }
                finally
                {
                    await track.DisposeAsync().ConfigureAwait(false);
                }
            }
        }
        finally
        {
            await _mediaStream.DisposeAsync().ConfigureAwait(false);
            _mediaStream = null;
        }
    }

    private static bool TryParseFacingMode(string value, out VideoFacingMode facingMode)
    {
        if (Enum.TryParse(value, ignoreCase: true, out facingMode))
        {
            return true;
        }

        facingMode = value.ToLowerInvariant() switch
        {
            "environment" => VideoFacingMode.Environment,
            "user" => VideoFacingMode.User,
            "left" => VideoFacingMode.Left,
            "right" => VideoFacingMode.Right,
            _ => VideoFacingMode.User
        };

        return string.Equals(value, "environment", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "user", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "left", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "right", StringComparison.OrdinalIgnoreCase);
    }

    public sealed class FrameData
    {
        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("pixels")]
        public byte[]? Pixels { get; set; }
    }

    private sealed class DisplayMediaStreamOptions
    {
        [JsonPropertyName("video")]
        public DisplayMediaVideoOptions Video { get; set; } = new();

        [JsonPropertyName("audio")]
        public bool Audio { get; set; }
    }

    private sealed class DisplayMediaVideoOptions
    {
        [JsonPropertyName("frameRate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? FrameRate { get; set; }

        [JsonPropertyName("width")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Height { get; set; }
    }
}
