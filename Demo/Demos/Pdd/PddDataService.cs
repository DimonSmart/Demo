using System.Text.Json;

namespace Demo.Demos.Pdd;

public class PddDataService(HttpClient httpClient, ILogger<PddDataService> logger) : IPddDataService
{
    private PddDatabase? _cachedDatabase;

    public async Task<PddDatabase> LoadDatabaseAsync()
    {
        if (_cachedDatabase != null)
        {
            return _cachedDatabase;
        }

        try
        {
            logger.LogInformation("Loading PDD database from remote URL");
            var response = await httpClient.GetAsync("https://DimonSmart.github.io/DGT/pdd-v2.json");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStreamAsync();
            var database = await JsonSerializer.DeserializeAsync<PddDatabase>(json) ?? new PddDatabase();
            _cachedDatabase = database;
            return database;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading PDD database");
            return new PddDatabase();
        }
    }
}