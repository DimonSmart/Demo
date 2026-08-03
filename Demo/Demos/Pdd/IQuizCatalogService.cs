namespace Demo.Demos.Pdd;

public interface IQuizCatalogService
{
    Task<IReadOnlyList<QuizCatalogEntry>> GetAllAsync();
    Task<QuizCatalogEntry?> FindBySourceIdAsync(string sourceId);
    Task<QuizCatalogEntry?> FindByUrlAsync(string url);
    Task<QuizCatalogEntry> AddOrUpdateAsync(QuizCatalogEntry entry);
    Task RemoveAsync(string sourceId);
}
