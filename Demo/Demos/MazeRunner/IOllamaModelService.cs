namespace Demo.Demos.MazeRunner
{
    public interface IOllamaModelService
    {
        /// <summary>
        /// Retrieves the list of loaded models from Ollama.
        /// Returns a simplified list containing ModelId, ParameterCount, and Size.
        /// </summary>
        Task<List<OllamaModelInfo>> GetLoadedModelsAsync();
    }
}