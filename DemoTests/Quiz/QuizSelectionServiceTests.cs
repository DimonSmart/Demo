using Demo.Demos.Quiz;

namespace DemoTests.Quiz;

public class QuizSelectionServiceTests
{
    private readonly QuizSelectionService service = new();

    [Fact]
    public void Select_UsesLastPracticedSource_WhenItIsRegistered() =>
        Assert.Equal("external", service.Select(Sources(), null, "external")?.SourceId);

    [Fact]
    public void Select_UsesBuiltInFallback_WhenSavedSourceIsMissing() =>
        Assert.Equal("builtin-pdd", service.Select(Sources(), null, "removed")?.SourceId);

    [Fact]
    public void Select_GivesRequestedSourcePriorityOverLastPracticedSource() =>
        Assert.Equal("builtin-pdd", service.Select(Sources(), "builtin-pdd", "external")?.SourceId);

    private static IReadOnlyList<QuizCatalogEntry> Sources() =>
    [
        new QuizCatalogEntry { SourceId = "builtin-pdd", IsBuiltIn = true },
        new QuizCatalogEntry { SourceId = "external", Url = "https://example.test/quiz.json" }
    ];
}
