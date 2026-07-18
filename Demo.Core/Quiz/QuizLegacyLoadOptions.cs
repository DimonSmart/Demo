namespace Demo.Core.Quiz;

public sealed class QuizLegacyLoadOptions
{
    public string DocumentId { get; init; } = string.Empty;
    public LocalizedText Title { get; init; } = new();
    public string? ImagesBaseUrl { get; init; }
}
