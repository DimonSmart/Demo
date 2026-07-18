namespace Demo.Core.Quiz;

public sealed class QuizDocument
{
    public const string SupportedSchemaVersion = "1.0";

    public string SchemaVersion { get; init; } = string.Empty;
    public string Id { get; init; } = string.Empty;
    public LocalizedText Title { get; init; } = new();
    public IReadOnlyList<string> Languages { get; init; } = [];
    public string? ImagesBaseUrl { get; init; }
    public IReadOnlyList<QuizTopic> Topics { get; init; } = [];
    public IReadOnlyList<QuizQuestion> Questions { get; init; } = [];
}
