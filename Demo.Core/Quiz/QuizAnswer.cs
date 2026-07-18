namespace Demo.Core.Quiz;

public sealed class QuizAnswer
{
    public string Id { get; init; } = string.Empty;
    public LocalizedText Text { get; init; } = new();
    public bool IsCorrect { get; init; }
}
