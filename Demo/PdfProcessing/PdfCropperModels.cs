using DimonSmart.PdfCropper;

namespace Demo.PdfProcessing;

internal sealed class PdfCropperOptions
{
    public CropMethod Method { get; set; } = CropMethod.ContentBased;
    public decimal Margin { get; set; } = 0.5m;
    public bool ExcludeEdgeTouchingObjects { get; set; } = true;
    public bool DetectRepeatedObjects { get; set; }
    public decimal RepeatedObjectOccurrenceThreshold { get; set; } = 40m;
    public int RepeatedObjectMinimumPageCount { get; set; } = 3;
    public string CompressionLevelKey { get; set; } = string.Empty;
    public string TargetPdfVersionValue { get; set; } = string.Empty;
    public bool EnableFullCompression { get; set; }
    public bool EnableSmartMode { get; set; }
    public bool RemoveUnusedObjects { get; set; }
    public bool RemoveXmpMetadata { get; set; }
    public bool ClearDocumentInfo { get; set; }
    public bool RemoveEmbeddedStandardFonts { get; set; }
    public bool MergeDuplicateFontSubsets { get; set; }
    public bool MergeIntoSingleDocument { get; set; }
}

internal sealed record CompressionOption(string Label, string Value, int? Level);

internal sealed record TargetVersionOption(string Label, string Value, PdfCompatibilityLevel? Level);

internal sealed record UploadedPdf(string Name, byte[] Content, int PageCount);
