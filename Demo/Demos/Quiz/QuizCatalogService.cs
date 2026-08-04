using Blazored.LocalStorage;

namespace Demo.Demos.Quiz;

public sealed class QuizCatalogService(ILocalStorageService localStorage, IConfiguration configuration) : IQuizCatalogService
{
    private const string StorageKey = "quizCatalog:v1";
    public async Task<IReadOnlyList<QuizCatalogEntry>> GetAllAsync()
    {
        var custom = await localStorage.GetItemAsync<List<QuizCatalogEntry>>(StorageKey) ?? [];
        return [BuiltIn(), .. custom];
    }
    public async Task<QuizCatalogEntry?> FindBySourceIdAsync(string sourceId) =>
        (await GetAllAsync()).FirstOrDefault(x => x.SourceId == sourceId);
    public async Task<QuizCatalogEntry?> FindByUrlAsync(string url)
    {
        var normalized = NormalizeUrl(url);
        return (await GetAllAsync()).FirstOrDefault(x => x.NormalizedUrl == normalized);
    }
    public async Task<QuizCatalogEntry> AddOrUpdateAsync(QuizCatalogEntry entry)
    {
        if (entry.IsBuiltIn) return BuiltIn();
        var normalized = NormalizeUrl(entry.Url);
        var entries = await localStorage.GetItemAsync<List<QuizCatalogEntry>>(StorageKey) ?? [];
        var old = entries.FindIndex(x => x.NormalizedUrl == normalized || x.SourceId == entry.SourceId);
        var saved = new QuizCatalogEntry { SourceId = string.IsNullOrWhiteSpace(entry.SourceId) ? Guid.NewGuid().ToString("N") : entry.SourceId, Url = entry.Url, NormalizedUrl = normalized, AddedAtUtc = entry.AddedAtUtc == default ? DateTime.UtcNow : entry.AddedAtUtc, LastKnownDocumentId = entry.LastKnownDocumentId, LastKnownTitle = entry.LastKnownTitle, LastKnownQuestionCount = entry.LastKnownQuestionCount, LastKnownDocumentHash = entry.LastKnownDocumentHash, LastOpenedAtUtc = entry.LastOpenedAtUtc, LastSuccessfulLoadAtUtc = entry.LastSuccessfulLoadAtUtc };
        if (old >= 0) entries[old] = saved; else entries.Add(saved);
        await localStorage.SetItemAsync(StorageKey, entries);
        return saved;
    }
    public async Task RemoveAsync(string sourceId)
    {
        if (sourceId == "builtin-pdd") throw new InvalidOperationException("The built-in quiz cannot be removed.");
        var entries = await localStorage.GetItemAsync<List<QuizCatalogEntry>>(StorageKey) ?? [];
        entries.RemoveAll(x => x.SourceId == sourceId);
        await localStorage.SetItemAsync(StorageKey, entries);
    }
    public static string NormalizeUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) throw new ArgumentException("The quiz URL is invalid.");
        if (uri.Scheme is not ("http" or "https")) throw new ArgumentException("Only HTTP and HTTPS quiz URLs are allowed.");
        if (!string.IsNullOrEmpty(uri.UserInfo)) throw new ArgumentException("Quiz URLs with user info are not allowed.");
        var builder = new UriBuilder(uri) { Fragment = string.Empty };
        return builder.Uri.AbsoluteUri;
    }
    private QuizCatalogEntry BuiltIn()
    {
        var url = configuration["Quiz:DocumentUrl"] ?? throw new InvalidOperationException("Quiz:DocumentUrl is not configured.");
        return new QuizCatalogEntry { SourceId = "builtin-pdd", Url = url, NormalizedUrl = NormalizeUrl(url), IsBuiltIn = true, AddedAtUtc = DateTime.UnixEpoch, LastKnownTitle = "Spanish driving theory test" };
    }
}
