namespace Demo.Demos.Quiz;

public sealed class QuizSelectionService
{
    public QuizCatalogEntry? Select(IReadOnlyList<QuizCatalogEntry> sources, string? requestedSourceId, string? lastPracticedSourceId)
    {
        return sources.FirstOrDefault(source => source.SourceId == requestedSourceId)
            ?? sources.FirstOrDefault(source => source.SourceId == lastPracticedSourceId)
            ?? sources.FirstOrDefault(source => source.IsBuiltIn)
            ?? sources.FirstOrDefault();
    }
}
