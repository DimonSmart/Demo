namespace Demo.Core.Quiz;

public sealed class QuizTopic
{
    public string Id { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public LocalizedText Title { get; init; } = new();
}
