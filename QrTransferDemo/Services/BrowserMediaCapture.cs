using System;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace QrTransferDemo.Services;

[SupportedOSPlatform("browser")]
public sealed partial class BrowserMediaCapture : IAsyncDisposable
{
    private JSObject? _videoElement;
    private JSObject? _canvasElement;
    private JSObject? _canvasContext;
    private JSObject? _mediaStream;
    private int _canvasWidth;
    private int _canvasHeight;

    public async Task InitializeAsync(string videoElementId, string canvasElementId, CancellationToken cancellationToken = default)
    {
        if (_videoElement is not null)
        {
            return;
        }

        await Task.Yield();
        _videoElement = DomInterop.GetElementById(videoElementId);
        _canvasElement = DomInterop.GetElementById(canvasElementId);

        if (_videoElement is null || _canvasElement is null)
        {
            throw new InvalidOperationException("Capture preview elements are not available.");
        }

        var getContext = _canvasElement.GetPropertyAsJSObject("getContext");
        ArgumentNullException.ThrowIfNull(getContext);
        try
        {
            _canvasContext = (JSObject?)JsInterop.ReflectApply(
                getContext,
                _canvasElement,
                new object?[] { "2d" }) ?? throw new InvalidOperationException("Unable to acquire 2D context.");
        }
        finally
        {
            getContext.Dispose();
        }

        _videoElement.SetProperty("autoplay", true);
        _videoElement.SetProperty("muted", true);
        _videoElement.SetProperty("playsInline", true);
    }

    public async Task StartCaptureAsync(CaptureSource source, CaptureOptions options, CancellationToken cancellationToken)
    {
        if (_videoElement is null)
        {
            throw new InvalidOperationException("Capture has not been initialized.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        JSObject stream;
        switch (source)
        {
            case CaptureSource.Screen:
                var screenConstraints = new
                {
                    video = new
                    {
                        frameRate = options.FrameRateHint
                    },
                    audio = false
                };
                stream = await MediaDevicesInterop.GetDisplayMediaAsync(screenConstraints);
                break;
            case CaptureSource.Camera:
                var cameraConstraints = new
                {
                    video = new
                    {
                        facingMode = options.FacingMode,
                        width = options.Width,
                        height = options.Height,
                        frameRate = options.FrameRateHint
                    },
                    audio = false
                };
                stream = await MediaDevicesInterop.GetUserMediaAsync(cameraConstraints);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(source), source, null);
        }

        _mediaStream?.Dispose();
        _mediaStream = stream;
        JsInterop.ReflectSet(_videoElement, "srcObject", stream);
        _canvasWidth = 0;
        _canvasHeight = 0;
    }

    public Task StopCaptureAsync()
    {
        if (_mediaStream is null)
        {
            return Task.CompletedTask;
        }

        try
        {
            var getTracks = _mediaStream.GetPropertyAsJSObject("getTracks");
            ArgumentNullException.ThrowIfNull(getTracks);
            var tracks = (JSObject?)JsInterop.ReflectApply(getTracks, _mediaStream, Array.Empty<object?>());
            getTracks.Dispose();

            if (tracks is not null)
            {
                var length = tracks.GetPropertyAsInt32("length");
                for (var i = 0; i < length; i++)
                {
                    var track = tracks.GetPropertyAsJSObject(i.ToString());
                    if (track is not null)
                    {
                        var stop = track.GetPropertyAsJSObject("stop");
                        ArgumentNullException.ThrowIfNull(stop);
                        JsInterop.ReflectApply(stop, track, Array.Empty<object?>());
                        stop.Dispose();
                        track.Dispose();
                    }
                }

                tracks.Dispose();
            }

            if (_videoElement is not null)
            {
                JsInterop.ReflectSet(_videoElement, "srcObject", null);
            }
        }
        finally
        {
            _mediaStream.Dispose();
            _mediaStream = null;
        }

        return Task.CompletedTask;
    }

    public bool TryCaptureFrame(out byte[]? pixels, out int width, out int height)
    {
        pixels = null;
        width = 0;
        height = 0;

        if (_videoElement is null || _canvasElement is null || _canvasContext is null)
        {
            return false;
        }

        var videoWidth = (int)Math.Round(_videoElement.GetPropertyAsDouble("videoWidth"));
        var videoHeight = (int)Math.Round(_videoElement.GetPropertyAsDouble("videoHeight"));
        if (videoWidth <= 0 || videoHeight <= 0)
        {
            return false;
        }

        if (videoWidth != _canvasWidth || videoHeight != _canvasHeight)
        {
            _canvasElement.SetProperty("width", videoWidth);
            _canvasElement.SetProperty("height", videoHeight);
            _canvasWidth = videoWidth;
            _canvasHeight = videoHeight;
        }

        var drawImage = _canvasContext.GetPropertyAsJSObject("drawImage");
        ArgumentNullException.ThrowIfNull(drawImage);
        JsInterop.ReflectApply(drawImage, _canvasContext, new object?[] { _videoElement, 0, 0, videoWidth, videoHeight });
        drawImage.Dispose();

        var getImageData = _canvasContext.GetPropertyAsJSObject("getImageData");
        ArgumentNullException.ThrowIfNull(getImageData);
        var imageData = (JSObject?)JsInterop.ReflectApply(getImageData, _canvasContext, new object?[] { 0, 0, videoWidth, videoHeight });
        getImageData.Dispose();

        if (imageData is null)
        {
            return false;
        }

        try
        {
            pixels = imageData.GetPropertyAsByteArray("data");
            if (pixels is null || pixels.Length == 0)
            {
                return false;
            }

            width = videoWidth;
            height = videoHeight;
            return true;
        }
        finally
        {
            imageData.Dispose();
        }
    }

    public void Dispose()
    {
        _mediaStream?.Dispose();
        _canvasContext?.Dispose();
        _canvasElement?.Dispose();
        _videoElement?.Dispose();
        _mediaStream = null;
        _canvasContext = null;
        _canvasElement = null;
        _videoElement = null;
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    public readonly record struct CaptureOptions(double? FrameRateHint, int? Width, int? Height, string? FacingMode);

    public enum CaptureSource
    {
        Screen,
        Camera
    }
}

[SupportedOSPlatform("browser")]
internal static partial class MediaDevicesInterop
{
    [JSImport("globalThis.navigator.mediaDevices.getDisplayMedia")]
    public static partial Task<JSObject> GetDisplayMediaAsync([JSMarshalAs<JSType.Any>] object? constraints);

    [JSImport("globalThis.navigator.mediaDevices.getUserMedia")]
    public static partial Task<JSObject> GetUserMediaAsync([JSMarshalAs<JSType.Any>] object constraints);
}

[SupportedOSPlatform("browser")]
internal static partial class DomInterop
{
    [JSImport("globalThis.document.getElementById")]
    public static partial JSObject GetElementById(string id);
}

[SupportedOSPlatform("browser")]
internal static partial class JsInterop
{
    [JSImport("globalThis.Reflect.apply")]
    [return: JSMarshalAs<JSType.Any>]
    public static partial object? ReflectApply(JSObject function, JSObject thisArg, [JSMarshalAs<JSType.Array<JSType.Any>>] object?[] arguments);

    [JSImport("globalThis.Reflect.set")]
    public static partial bool ReflectSet(JSObject target, string propertyKey, [JSMarshalAs<JSType.Any>] object? value);

    [JSImport("globalThis.Reflect.construct")]
    public static partial JSObject ReflectConstruct(JSObject target, [JSMarshalAs<JSType.Array<JSType.Any>>] object?[] arguments);

    [JSImport("globalThis.URL.createObjectURL")]
    public static partial string CreateObjectUrl(JSObject obj);

    [JSImport("globalThis.URL.revokeObjectURL")]
    public static partial void RevokeObjectUrl(string url);
}
