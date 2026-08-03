using Demo.Core.Quiz;
namespace Demo.Demos.Pdd;
public interface IQuizSessionService { QuizCatalogEntry? CurrentSource { get; } QuizDocument? CurrentDocument { get; } LoadedQuizDocument? Current { get; } Task<QuizSynchronizationResult> StartAsync(QuizCatalogEntry source, CancellationToken cancellationToken = default); void Clear(); }
public sealed class QuizSessionService(IQuizSourceLoader loader, IQuizCatalogService catalog, QuizProgressService progress) : IQuizSessionService
{
    public QuizCatalogEntry? CurrentSource { get; private set; } public QuizDocument? CurrentDocument => Current?.Document; public LoadedQuizDocument? Current { get; private set; }
    public async Task<QuizSynchronizationResult> StartAsync(QuizCatalogEntry source, CancellationToken cancellationToken=default)
    {
        var normalizedUrl = QuizCatalogService.NormalizeUrl(source.Url);
        var candidate = new QuizCatalogEntry { SourceId = string.IsNullOrWhiteSpace(source.SourceId) ? Guid.NewGuid().ToString("N") : source.SourceId, Url = source.Url, NormalizedUrl = normalizedUrl, IsBuiltIn = source.IsBuiltIn, AddedAtUtc = source.AddedAtUtc == default ? DateTime.UtcNow : source.AddedAtUtc };
        var loaded=await loader.LoadAsync(candidate,cancellationToken); var sync=await progress.SynchronizeAsync(loaded); Current=loaded;
        CurrentSource=await catalog.AddOrUpdateAsync(new QuizCatalogEntry { SourceId=candidate.SourceId, Url=candidate.Url, IsBuiltIn=candidate.IsBuiltIn, AddedAtUtc=candidate.AddedAtUtc, LastKnownDocumentId=loaded.Document.Id, LastKnownTitle=loaded.Document.Title.Values.Values.FirstOrDefault(), LastKnownQuestionCount=loaded.Document.Questions.Count, LastKnownDocumentHash=loaded.DocumentHash, LastOpenedAtUtc=DateTime.UtcNow, LastSuccessfulLoadAtUtc=DateTime.UtcNow }); return sync.Result;
    }
    public void Clear() { Current=null; CurrentSource=null; }
}
