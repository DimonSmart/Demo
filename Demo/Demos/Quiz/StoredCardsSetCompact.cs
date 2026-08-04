namespace Demo.Demos.Quiz;

public sealed record StoredCardsSetCompact(string FormatVersion, string QuizDocumentId, List<string[]> Cards);
