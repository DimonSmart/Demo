namespace Demo.Demos.Pdd;

public sealed class QuestionItem
{
    public string Id { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string TopicId { get; init; } = string.Empty;
    public LocalizedText Text { get; init; } = new();
    public List<AnswerItem> Answers { get; init; } = [];
    public LocalizedText? Explanation { get; init; }
    public string? Image { get; init; }
    public List<LocalizedText>? Terms { get; init; }
    public List<SourceReference>? SourceReferences { get; init; }
}

public sealed class SourceReference
{
    public string Pointer { get; init; } = string.Empty;
    public LocalizedText? Quote { get; init; }
}
