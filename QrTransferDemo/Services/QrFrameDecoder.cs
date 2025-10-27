using System;
using ZXing;
using ZXing.Common;

namespace QrTransferDemo.Services;

public sealed class QrFrameDecoder
{
    private readonly object _sync = new();
    private readonly IBarcodeReaderGeneric _reader;

    public QrFrameDecoder()
    {
        _reader = new BarcodeReaderGeneric
        {
            AutoRotate = true,
            Options = new DecodingOptions
            {
                PossibleFormats = new[] { BarcodeFormat.QR_CODE },
                TryHarder = true,
                ReturnCodabarStartEnd = false,
                TryInverted = true
            }
        };
    }

    public bool TryDecode(byte[] rgbaPixels, int width, int height, out byte[]? payload)
    {
        payload = null;
        if (rgbaPixels is null || width <= 0 || height <= 0)
        {
            return false;
        }

        var expectedLength = width * height * 4;
        if (rgbaPixels.Length < expectedLength)
        {
            return false;
        }

        byte[] buffer;
        if (rgbaPixels.Length == expectedLength)
        {
            buffer = rgbaPixels;
        }
        else
        {
            buffer = new byte[expectedLength];
            Array.Copy(rgbaPixels, buffer, expectedLength);
        }

        lock (_sync)
        {
            try
            {
                var luminance = new RGBLuminanceSource(buffer, width, height, RGBLuminanceSource.BitmapFormat.RGBA32);
                if (luminance == null)
                {
                    Console.WriteLine($"[QrFrameDecoder] luminance is null! buffer.Length={buffer.Length}, width={width}, height={height}");
                    return false;
                }

                var result = _reader.Decode(luminance);
                if (result is { RawBytes.Length: > 0 })
                {
                    payload = result.RawBytes;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[QrFrameDecoder] Error during decode: {ex.Message}");
                return false;
            }
        }

        return false;
    }
}
