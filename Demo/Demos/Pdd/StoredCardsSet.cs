namespace Demo.Demos.Pdd;

public sealed class StoredCardsSet
{
    public string FormatVersion { get; init; } = "2.0";
    public string QuizDocumentId { get; init; } = string.Empty;
    public IReadOnlyCollection<QuestionStudyCard> Cards { get; init; } = [];
}
