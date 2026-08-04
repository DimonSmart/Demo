namespace Demo.Demos.Quiz;

public sealed class QuizCatalogEntry
{
    public string SourceId { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string NormalizedUrl { get; init; } = string.Empty;
    public bool IsBuiltIn { get; init; }
    public string? LastKnownDocumentId { get; init; }
    public string? LastKnownTitle { get; init; }
    public int? LastKnownQuestionCount { get; init; }
    public string? LastKnownDocumentHash { get; init; }
    public DateTime AddedAtUtc { get; init; }
    public DateTime? LastOpenedAtUtc { get; init; }
    public DateTime? LastSuccessfulLoadAtUtc { get; init; }
}
