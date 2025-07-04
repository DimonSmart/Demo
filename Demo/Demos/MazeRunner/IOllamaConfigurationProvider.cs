namespace Demo.Demos.MazeRunner
{
    /// <summary>
    /// Provides Ollama configuration settings
    /// </summary>
    public interface IOllamaConfigurationProvider
    {
        Task<string> GetServerUrlAsync();

        Task<string?> GetPasswordAsync();
    }
}
