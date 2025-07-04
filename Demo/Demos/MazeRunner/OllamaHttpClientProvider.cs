namespace Demo.Demos.MazeRunner
{
    public class OllamaHttpClientProvider(IHttpClientFactory httpClientFactory, IOllamaConfigurationProvider configProvider) : IOllamaHttpClientProvider
    {
        public async Task<HttpClient> CreateConfiguredHttpClientAsync()
        {
            var serverUrl = await configProvider.GetServerUrlAsync();
            var password = await configProvider.GetPasswordAsync();

            var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(serverUrl ?? "http://localhost:11434");
            client.Timeout = TimeSpan.FromMinutes(20);

            if (!string.IsNullOrEmpty(password))
            {
                var authString = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($":{password}"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authString);
            }

            return client;
        }
    }
}
