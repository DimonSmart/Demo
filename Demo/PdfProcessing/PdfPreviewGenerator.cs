using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using iText.Kernel.Pdf;
using PDFtoImage;
using SkiaSharp;

namespace Demo.PdfProcessing;

internal static class PdfPreviewGenerator
{
    public static IReadOnlyList<string> GeneratePreviews(byte[] pdfBytes, int maxPages, double scale, CancellationToken cancellationToken = default)
    {
        if (pdfBytes == null)
        {
            throw new ArgumentNullException(nameof(pdfBytes));
        }

        if (maxPages <= 0)
        {
            return Array.Empty<string>();
        }

        var normalizedScale = Math.Clamp(scale, 0.5, 3.0);
        var previews = new List<string>();

        var pageCount = GetPageCount(pdfBytes);
        var pageLimit = Math.Min(maxPages, pageCount);

        for (var pageIndex = 0; pageIndex < pageLimit; pageIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var bitmap = RenderPageToBitmap(pdfBytes, pageIndex);
            using var scaledBitmap = CreateScaledBitmap(bitmap, normalizedScale);

            var bitmapForEncoding = scaledBitmap ?? bitmap;
            var dataUrl = EncodeToPngDataUrl(bitmapForEncoding);
            previews.Add(dataUrl);
        }

        return previews;
    }

    private static int GetPageCount(byte[] pdfBytes)
    {
        using var stream = new MemoryStream(pdfBytes, writable: false);
        using var reader = new PdfReader(stream);
        using var document = new PdfDocument(reader);
        return document.GetNumberOfPages();
    }

    private static SKBitmap? CreateScaledBitmap(SKBitmap source, double scale)
    {
        if (Math.Abs(scale - 1d) < 0.001)
        {
            return null;
        }

        var width = Math.Max(1, (int)Math.Round(source.Width * scale));
        var height = Math.Max(1, (int)Math.Round(source.Height * scale));

        var resized = new SKBitmap(width, height, source.ColorType, source.AlphaType);
        var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None);
        if (!source.ScalePixels(resized, sampling))
        {
            resized.Dispose();
            throw new InvalidOperationException("Failed to scale PDF preview image.");
        }

        return resized;
    }

    private static SKBitmap RenderPageToBitmap(byte[] pdfBytes, int pageIndex)
    {
#pragma warning disable CA1416
        return Conversion.ToImage(pdfBytes, page: pageIndex);
#pragma warning restore CA1416
    }

    private static string EncodeToPngDataUrl(SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, quality: 100);
        var bytes = data.ToArray();
        var base64 = Convert.ToBase64String(bytes);
        return $"data:image/png;base64,{base64}";
    }
}
