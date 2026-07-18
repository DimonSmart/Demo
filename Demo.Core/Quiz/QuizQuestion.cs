namespace Demo.Core.Quiz;

public sealed class QuizQuestion
{
    public string Id { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string TopicId { get; init; } = string.Empty;
    public LocalizedText Text { get; init; } = new();
    public IReadOnlyList<QuizAnswer> Answers { get; init; } = [];
    public LocalizedText? Explanation { get; init; }
    public string? Image { get; init; }
    public IReadOnlyList<LocalizedText>? Terms { get; init; }
    public IReadOnlyList<QuizSourceReference>? SourceReferences { get; init; }
}
