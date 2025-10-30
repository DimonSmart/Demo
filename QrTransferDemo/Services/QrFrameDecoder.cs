using System;
using System.Text;
using ZXingCpp;

namespace QrTransferDemo.Services;

public sealed class QrFrameDecoder
{
    private readonly object _sync = new();
    private readonly BarcodeReader _reader;

    public QrFrameDecoder()
    {
        _reader = new BarcodeReader
        {
            TryHarder = true,
            TryRotate = true,
            TryInvert = true,
            Formats = BarcodeFormats.QRCode
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
                var imageView = new ImageView(buffer, width, height, ImageFormat.RGBA, width * 4, 4);
                var results = _reader.From(imageView);
                if (results is null || results.Length == 0)
                {
                    return false;
                }

                foreach (var result in results)
                {
                    if (result is null || !result.IsValid)
                    {
                        continue;
                    }

                    if ((result.Format & BarcodeFormats.QRCode) == 0)
                    {
                        continue;
                    }

                    if (result.Bytes is { Length: > 0 } bytes)
                    {
                        payload = CopyBytes(bytes);
                        return true;
                    }

                    if (result.BytesECI is { Length: > 0 } bytesEci)
                    {
                        payload = CopyBytes(bytesEci);
                        return true;
                    }

                    if (!string.IsNullOrEmpty(result.Text))
                    {
                        payload = Encoding.UTF8.GetBytes(result.Text);
                        return true;
                    }
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

    private static byte[] CopyBytes(byte[] source)
    {
        var copy = new byte[source.Length];
        Buffer.BlockCopy(source, 0, copy, 0, source.Length);
        return copy;
    }
}
