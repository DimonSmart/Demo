namespace Demo.PdfProcessing;

/// <summary>
/// Represents configuration for PDF cropping operations.
/// </summary>
public readonly struct CropSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CropSettings"/> struct.
    /// </summary>
    /// <param name="method">Cropping method to use.</param>
    /// <param name="excludeEdgeTouchingObjects">
    /// Whether to ignore content that touches current page boundaries when analyzing content bounds.
    /// </param>
    /// <param name="margin">Safety margin in points to add around detected content bounds.</param>
    public CropSettings(
        CropMethod method,
        bool excludeEdgeTouchingObjects = false,
        float margin = 0.5f,
        float edgeExclusionTolerance = 1f,
        bool detectRepeatedObjects = false,
        double repeatedObjectOccurrenceThreshold = 40,
        int repeatedObjectMinimumPageCount = 3)
    {
        Method = method;
        ExcludeEdgeTouchingObjects = excludeEdgeTouchingObjects;
        Margin = margin;
        EdgeExclusionTolerance = edgeExclusionTolerance;
        DetectRepeatedObjects = detectRepeatedObjects;
        RepeatedObjectOccurrenceThreshold = repeatedObjectOccurrenceThreshold;
        RepeatedObjectMinimumPageCount = repeatedObjectMinimumPageCount;
    }

    /// <summary>
    /// Gets the cropping method to use.
    /// </summary>
    public CropMethod Method { get; }

    /// <summary>
    /// Gets a value indicating whether edge-touching content should be ignored during content analysis.
    /// </summary>
    public bool ExcludeEdgeTouchingObjects { get; }

    /// <summary>
    /// Gets the safety margin in points to add around detected content bounds.
    /// </summary>
    public float Margin { get; }

    /// <summary>
    /// Gets the tolerance distance (in points) for classifying content as touching a page edge.
    /// </summary>
    public float EdgeExclusionTolerance { get; }

    /// <summary>
    /// Gets a value indicating whether repeated content objects should be excluded from bounds detection.
    /// </summary>
    public bool DetectRepeatedObjects { get; }

    /// <summary>
    /// Gets the minimum percentage of analyzed pages on which an object must appear to be considered repeated.
    /// </summary>
    public double RepeatedObjectOccurrenceThreshold { get; }

    /// <summary>
    /// Gets the minimum number of pages required in a document before repeated object detection is applied.
    /// </summary>
    public int RepeatedObjectMinimumPageCount { get; }

    /// <summary>
    /// Gets the default cropping settings.
    /// </summary>
    public static CropSettings Default => new(CropMethod.ContentBased);
}
