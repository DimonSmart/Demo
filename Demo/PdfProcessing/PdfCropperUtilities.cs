using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DimonSmart.PdfCropper;
using iText.Kernel.Pdf;

namespace Demo.PdfProcessing;

internal static class PdfCropperUtilities
{
    private static readonly string[] SizeLabels = { "B", "KB", "MB", "GB" };

    public static string FormatFileSize(long bytes)
    {
        if (bytes <= 0)
        {
            return "0 B";
        }

        var order = (int)Math.Floor(Math.Log(bytes, 1024));
        order = Math.Clamp(order, 0, SizeLabels.Length - 1);
        var adjusted = bytes / Math.Pow(1024, order);
        return string.Format(CultureInfo.CurrentCulture, "{0:F1} {1}", adjusted, SizeLabels[order]);
    }

    public static string GetMethodLabel(CropMethod method)
    {
        return method switch
        {
            CropMethod.ContentBased => "Content analysis (faster)",
            CropMethod.BitmapBased => "Rasterization (more precise)",
            _ => method.ToString()
        };
    }

    public static int GetPageCount(byte[] pdfBytes)
    {
        using var stream = new MemoryStream(pdfBytes, writable: false);
        using var reader = new PdfReader(stream);
        using var document = new PdfDocument(reader);
        return document.GetNumberOfPages();
    }

    public static byte[] MergeOriginalDocuments(IReadOnlyList<byte[]> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        if (documents.Count == 0)
        {
            return Array.Empty<byte>();
        }

        using var outputStream = new MemoryStream();
        using var writer = new PdfWriter(outputStream);
        using var mergedDocument = new PdfDocument(writer);

        foreach (var bytes in documents)
        {
            using var inputStream = new MemoryStream(bytes, writable: false);
            using var reader = new PdfReader(inputStream);
            using var pdfDocument = new PdfDocument(reader);
            pdfDocument.CopyPagesTo(1, pdfDocument.GetNumberOfPages(), mergedDocument);
        }

        mergedDocument.Close();
        return outputStream.ToArray();
    }
}
