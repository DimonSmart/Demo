namespace Demo.Demos.Pdd;

public sealed class AnswerItem
{
    public string Id { get; init; } = string.Empty;
    public LocalizedText Text { get; init; } = new();
    public bool IsCorrect { get; init; }
}
