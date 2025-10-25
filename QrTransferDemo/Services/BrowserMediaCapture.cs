using System;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace QrTransferDemo.Services;

[SupportedOSPlatform("browser")]
public sealed partial class BrowserMediaCapture : IAsyncDisposable
{
    private const string ModuleName = "_content/QrTransferDemo/browserMediaCapture.js";

    private int? _contextId;
    private int _lastFrameVersion;

    public async Task InitializeAsync(string videoElementId, string canvasElementId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_contextId.HasValue)
        {
            return;
        }

        await Task.Yield();

        _contextId = CreateContext(videoElementId, canvasElementId);
        _lastFrameVersion = 0;
    }

    public async Task StartCaptureAsync(CaptureSource source, CaptureOptions options, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_contextId is null)
        {
            throw new InvalidOperationException("Capture has not been initialized.");
        }

        StopCaptureInternal();

        await StartCaptureInternalAsync(
            _contextId.Value,
            (int)source,
            options.FrameRateHint,
            options.Width,
            options.Height,
            options.FacingMode);
        cancellationToken.ThrowIfCancellationRequested();

        _lastFrameVersion = 0;
    }

    public Task StopCaptureAsync()
    {
        StopCaptureInternal();
        return Task.CompletedTask;
    }

    public bool TryCaptureFrame(out byte[]? pixels, out int width, out int height)
    {
        pixels = null;
        width = 0;
        height = 0;

        if (_contextId is null)
        {
            return false;
        }

        var frame = TryCaptureFrameInternal(_contextId.Value, _lastFrameVersion);
        if (frame is null)
        {
            return false;
        }

        using (frame)
        {
            var versionValue = frame.GetPropertyAsDouble("version");
            var version = double.IsNaN(versionValue) ? 0 : (int)Math.Round(versionValue);
            if (version == 0 || version == _lastFrameVersion)
            {
                return false;
            }

            var widthValue = frame.GetPropertyAsDouble("width");
            var heightValue = frame.GetPropertyAsDouble("height");
            width = double.IsNaN(widthValue) ? 0 : (int)Math.Round(widthValue);
            height = double.IsNaN(heightValue) ? 0 : (int)Math.Round(heightValue);

            if (width <= 0 || height <= 0)
            {
                width = 0;
                height = 0;
                return false;
            }

            pixels = frame.GetPropertyAsByteArray("pixels");
            if (pixels is null || pixels.Length == 0)
            {
                width = 0;
                height = 0;
                pixels = null;
                return false;
            }

            _lastFrameVersion = version;
            return true;
        }
    }

    public void Dispose()
    {
        if (_contextId is null)
        {
            return;
        }

        StopCaptureInternal();
        DisposeContext(_contextId.Value);
        _contextId = null;
        _lastFrameVersion = 0;
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    public readonly record struct CaptureOptions(double? FrameRateHint, int? Width, int? Height, string? FacingMode);

    public enum CaptureSource
    {
        Screen = 0,
        Camera = 1
    }

    [JSImport("createContext", ModuleName)]
    private static partial int CreateContext(string videoElementId, string canvasElementId);

    [JSImport("startCapture", ModuleName)]
    private static partial Task StartCaptureInternalAsync(int contextId, int source, double? frameRateHint, int? width, int? height, string? facingMode);

    [JSImport("stopCapture", ModuleName)]
    private static partial void StopCapture(int contextId);

    [JSImport("disposeContext", ModuleName)]
    private static partial void DisposeContext(int contextId);

    [JSImport("tryCaptureFrame", ModuleName)]
    private static partial JSObject? TryCaptureFrameInternal(int contextId, int knownVersion);

    private void StopCaptureInternal()
    {
        if (_contextId is null)
        {
            return;
        }

        StopCapture(_contextId.Value);
        _lastFrameVersion = 0;
    }
}
