using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using iText.Kernel.Pdf;

namespace Demo.PdfProcessing;

internal sealed class PdfProcessingResult
{
    private readonly List<string> _originalPreviewImages = new();
    private readonly List<string> _croppedPreviewImages = new();
    private byte[] _originalBytes;
    private byte[] _croppedBytes;
    private bool _previewsLoaded;

    public PdfProcessingResult(
        string originalName,
        byte[] originalBytes,
        long originalSize,
        int originalPageCount,
        byte[] croppedBytes,
        IReadOnlyList<string> logMessages,
        int previewLimit)
    {
        FileName = originalName;
        _originalBytes = originalBytes;
        _croppedBytes = croppedBytes;
        OriginalSize = originalSize;
        CroppedSize = croppedBytes.LongLength;
        SizeSavingsPercentage = OriginalSize == 0 ? 0 : 1 - (double)CroppedSize / OriginalSize;
        CroppedFileName = BuildCroppedFileName(originalName);
        OriginalPageCount = originalPageCount;
        OriginalPreviewCount = Math.Min(originalPageCount, previewLimit);
        CroppedPageCount = GetPageCount(croppedBytes);
        CroppedPreviewCount = Math.Min(CroppedPageCount, previewLimit);
        CroppedDownloadUrl = BuildDataUrl(croppedBytes);
        LogMessages = logMessages;
    }

    public string FileName { get; }

    public string CroppedFileName { get; }

    public long OriginalSize { get; }

    public long CroppedSize { get; }

    public double SizeSavingsPercentage { get; }

    public IReadOnlyList<string> OriginalPreviewImages => _originalPreviewImages;

    public IReadOnlyList<string> CroppedPreviewImages => _croppedPreviewImages;

    public int OriginalPageCount { get; }

    public int OriginalPreviewCount { get; }

    public int CroppedPageCount { get; }

    public int CroppedPreviewCount { get; }

    public string CroppedDownloadUrl { get; }

    public IReadOnlyList<string> LogMessages { get; }

    public bool IsOriginalPreviewLoading { get; private set; }

    public bool IsCroppedPreviewLoading { get; private set; }

    public string? OriginalPreviewError { get; private set; }

    public string? CroppedPreviewError { get; private set; }

    public void BeginPreviewGeneration()
    {
        if (_previewsLoaded)
        {
            return;
        }

        if (OriginalPreviewCount > 0)
        {
            IsOriginalPreviewLoading = true;
        }

        if (CroppedPreviewCount > 0)
        {
            IsCroppedPreviewLoading = true;
        }
    }

    public async Task GeneratePreviewsAsync(double scale, Func<string, Task>? progressCallback = null)
    {
        if (_previewsLoaded)
        {
            return;
        }

        try
        {
            if (IsOriginalPreviewLoading)
            {
                OriginalPreviewError = null;
                try
                {
                    if (progressCallback != null)
                    {
                        await progressCallback("Generating original previews...");
                    }

                    await Task.Yield();
                    var images = await PdfPreviewGenerator.GeneratePreviewsAsync(
                        _originalBytes,
                        OriginalPreviewCount,
                        scale,
                        progressCallback);
                    _originalPreviewImages.Clear();
                    _originalPreviewImages.AddRange(images);
                }
                catch (Exception ex)
                {
                    OriginalPreviewError = $"Failed to render the original preview: {ex.Message}";
                }

                IsOriginalPreviewLoading = false;
            }

            if (IsCroppedPreviewLoading)
            {
                CroppedPreviewError = null;
                try
                {
                    if (progressCallback != null)
                    {
                        await progressCallback("Generating cropped previews...");
                    }

                    await Task.Yield();
                    var images = await PdfPreviewGenerator.GeneratePreviewsAsync(
                        _croppedBytes,
                        CroppedPreviewCount,
                        scale,
                        progressCallback);
                    _croppedPreviewImages.Clear();
                    _croppedPreviewImages.AddRange(images);
                }
                catch (Exception ex)
                {
                    CroppedPreviewError = $"Failed to render the cropped preview: {ex.Message}";
                }

                IsCroppedPreviewLoading = false;
            }
        }
        finally
        {
            _originalBytes = Array.Empty<byte>();
            _croppedBytes = Array.Empty<byte>();
            _previewsLoaded = true;
        }
    }

    private static string BuildDataUrl(byte[] pdfBytes)
    {
        var base64 = Convert.ToBase64String(pdfBytes);
        return $"data:application/pdf;base64,{base64}";
    }

    private static string BuildCroppedFileName(string originalName)
    {
        var fileName = Path.GetFileNameWithoutExtension(originalName);
        var extension = Path.GetExtension(originalName);
        if (string.IsNullOrEmpty(fileName))
        {
            fileName = "document";
        }

        if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            extension = ".pdf";
        }

        return $"{fileName}-cropped{extension}";
    }

    private static int GetPageCount(byte[] pdfBytes)
    {
        using var stream = new MemoryStream(pdfBytes, writable: false);
        using var reader = new PdfReader(stream);
        using var document = new PdfDocument(reader);
        return document.GetNumberOfPages();
    }
}
