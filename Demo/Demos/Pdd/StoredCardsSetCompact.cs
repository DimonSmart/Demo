namespace Demo.Demos.Pdd;

public sealed record StoredCardsSetCompact(string FormatVersion, string QuizDocumentId, List<string[]> Cards);
