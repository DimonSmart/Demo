using System;
using System.Collections.Generic;
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

    public bool TryDecode(byte[] rgbaPixels, int width, int height, out byte[]? payload, out QrFrameDetectionInfo detection)
    {
        payload = null;
        detection = QrFrameDetectionInfo.None;
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
            var points = new List<ResultPoint>();
            void CapturePoint(ResultPoint point)
            {
                if (point is not null)
                {
                    points.Add(point);
                }
            }

            try
            {
                var callback = new ResultPointCallback(CapturePoint);
                _reader.Options.Hints[DecodeHintType.NEED_RESULT_POINT_CALLBACK] = callback;
                var luminance = new RGBLuminanceSource(buffer, width, height, RGBLuminanceSource.BitmapFormat.RGBA32);
                if (luminance == null)
                {
                    Console.WriteLine($"[QrFrameDecoder] luminance is null! buffer.Length={buffer.Length}, width={width}, height={height}");
                    detection = new QrFrameDetectionInfo(points.Count > 0, points.Count > 0, points.Count);
                    return false;
                }

                var result = _reader.Decode(luminance);
                var pointCount = points.Count;
                detection = new QrFrameDetectionInfo(pointCount > 0, result is null && pointCount > 0, pointCount);
                if (result is null)
                {
                    return false;
                }

                if (TryGetByteSegments(result.ResultMetadata, out var segments))
                {
                    var totalLength = 0;
                    foreach (var segment in segments)
                    {
                        if (segment is { Length: > 0 })
                        {
                            totalLength += segment.Length;
                        }
                    }

                    if (totalLength > 0)
                    {
                        var assembled = new byte[totalLength];
                        var offset = 0;
                        foreach (var segment in segments)
                        {
                            if (segment is { Length: > 0 })
                            {
                                Buffer.BlockCopy(segment, 0, assembled, offset, segment.Length);
                                offset += segment.Length;
                            }
                        }

                        payload = assembled;
                        return true;
                    }
                }

                if (result.RawBytes is { Length: > 0 })
                {
                    var bytes = result.RawBytes;
                    var copy = new byte[bytes.Length];
                    Buffer.BlockCopy(bytes, 0, copy, 0, bytes.Length);
                    payload = copy;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[QrFrameDecoder] Error during decode: {ex.Message}");
                detection = new QrFrameDetectionInfo(points.Count > 0, points.Count > 0, points.Count);
                return false;
            }
            finally
            {
                _reader.Options.Hints.Remove(DecodeHintType.NEED_RESULT_POINT_CALLBACK);
            }
        }

        return false;
    }

    private static bool TryGetByteSegments(IDictionary<ResultMetadataType, object>? metadata, out IList<byte[]> segments)
    {
        segments = Array.Empty<byte[]>();
        if (metadata is null || metadata.Count == 0)
        {
            return false;
        }

        if (!metadata.TryGetValue(ResultMetadataType.BYTE_SEGMENTS, out var raw) || raw is not IList<byte[]> list || list.Count == 0)
        {
            return false;
        }

        segments = list;
        return true;
    }
}

public readonly record struct QrFrameDetectionInfo(bool HasDetection, bool HasPartialDetection, int ResultPointCount)
{
    public static QrFrameDetectionInfo None => new(false, false, 0);
}
