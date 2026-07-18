using System.Text.Json;

namespace Demo.Demos.Pdd;

public sealed class PddDataService(HttpClient httpClient, IConfiguration configuration, ILogger<PddDataService> logger) : IPddDataService
{
    private PddDatabase? _cachedDatabase;

    public async Task<PddDatabase> LoadDatabaseAsync()
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
            var document = await JsonSerializer.DeserializeAsync<PddDatabase>(stream) ?? throw new InvalidDataException("Quiz document is empty.");
            QuizDocumentValidator.Validate(document);
            return _cachedDatabase = document;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to load quiz document");
            throw;
        }
    }
}
