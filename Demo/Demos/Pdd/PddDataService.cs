using Demo.Core.Quiz;

namespace Demo.Demos.Pdd;

public sealed class PddDataService(
    HttpClient httpClient,
    IConfiguration configuration,
    IQuizDocumentLoader quizDocumentLoader,
    ILogger<PddDataService> logger) : IPddDataService
{
    private QuizDocument? _cachedDatabase;

    public async Task<QuizDocument> LoadDatabaseAsync()
    {
        if (_cachedDatabase is not null) return _cachedDatabase;
        var documentUrl = configuration["Quiz:DocumentUrl"];
        if (string.IsNullOrWhiteSpace(documentUrl)) throw new InvalidOperationException("Quiz:DocumentUrl is not configured.");
        try
        {
            logger.LogInformation("Loading quiz document from {DocumentUrl}", documentUrl);
            using var response = await httpClient.GetAsync(documentUrl);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync();
            var document = await quizDocumentLoader.LoadAsync(stream, CreateLegacyOptions());
            return _cachedDatabase = document;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to load quiz document");
            throw;
        }
    }

    private QuizLegacyLoadOptions CreateLegacyOptions()
    {
        var legacyTitle = configuration.GetSection("Quiz:LegacyTitle").Get<Dictionary<string, string>>()
            ?? throw new InvalidOperationException("Quiz:LegacyTitle is not configured.");
        return new QuizLegacyLoadOptions
        {
            DocumentId = configuration["Quiz:LegacyDocumentId"] ?? throw new InvalidOperationException("Quiz:LegacyDocumentId is not configured."),
            ImagesBaseUrl = configuration["Quiz:LegacyImagesBaseUrl"],
            Title = new LocalizedText { Values = legacyTitle }
        };
    }
}
