using Demo.Demos.Common;

namespace Demo.Demos.MazeRunner
{
    public class OllamaConfigurationProvider(UserPreferencesStorageService<MazeRunnerUserPreferences> preferencesService) : IOllamaConfigurationProvider
    {
        public async Task<string> GetServerUrlAsync()
        {
            var preferences = await GetPreferencesAsync();
            return preferences?.OllamaServerUrl ?? "http://localhost:11434";
        }

        public async Task<string?> GetPasswordAsync()
        {
            var preferences = await GetPreferencesAsync();
            return preferences?.OllamaPassword;
        }

        private async Task<MazeRunnerUserPreferences?> GetPreferencesAsync()
        {
            try
            {
                return await preferencesService.LoadPreferencesAsync();
            }
            catch
            {
                return new MazeRunnerUserPreferences();
            }
        }
    }
}
