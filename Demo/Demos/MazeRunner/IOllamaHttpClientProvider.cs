namespace Demo.Demos.MazeRunner
{
    public interface IOllamaHttpClientProvider
    {
        Task<HttpClient> CreateConfiguredHttpClientAsync();
    }
}
