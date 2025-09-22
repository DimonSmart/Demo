using System.Text.Json;

namespace Demo.Demos.Pdd;

public class PddDataService(HttpClient httpClient, ILogger<PddDataService> logger) : IPddDataService
{
    public async Task<PddDatabase> LoadDatabaseAsync()
    {
        try
        {
#if DEBUG
            // In debug mode, first try to load from wwwroot/pdd-debug.json
            logger.LogInformation("Debug mode: Attempting to load PDD database from local debug file");
            try
            {
                var debugResponse = await httpClient.GetAsync("/pdd-debug.json");
                if (debugResponse.IsSuccessStatusCode)
                {
                    logger.LogInformation("Loading PDD database from debug file: /pdd-debug.json");
                    var debugJson = await debugResponse.Content.ReadAsStreamAsync();
                    return await JsonSerializer.DeserializeAsync<PddDatabase>(debugJson) ?? new PddDatabase();
                }
                else
                {
                    logger.LogInformation("Debug file not found or not accessible, falling back to remote URL");
                }
            }
            catch (Exception debugEx)
            {
                logger.LogWarning(debugEx, "Failed to load debug file, falling back to remote URL");
            }
#endif
            // Load from remote URL (production mode or debug fallback)
            logger.LogInformation("Loading PDD database from remote URL");
            var response = await httpClient.GetAsync("https://DimonSmart.github.io/DGT/pdd.json");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<PddDatabase>(json) ?? new PddDatabase();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading PDD database");
            return new PddDatabase();
        }
    }
}