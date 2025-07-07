using Microsoft.SemanticKernel;

namespace Demo.Demos.MazeRunner;
public static class KernelFactory
{
    public static Kernel BuildKernel(KernelBuildParameters parameters)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var builder = Kernel.CreateBuilder();

#pragma warning disable SKEXP0070

        switch (parameters.connectionType.ToLower())
        {
            case "ollama":
                var ollamaHandler = new HttpClientHandler();
                if (parameters.IgnoreSslErrors && !OperatingSystem.IsBrowser())
                {
                    ollamaHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                }

                var ollamaHttpClient = new HttpClient(new LoggingHttpMessageHandler(parameters.LogStore, ollamaHandler))
                {
                    BaseAddress = new Uri(parameters.OllamaServerUrl),
                    Timeout = TimeSpan.FromMinutes(20)
                };

                if (!string.IsNullOrEmpty(parameters.OllamaPassword))
                {
                    var authString = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($":{parameters.OllamaPassword}"));
                    ollamaHttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authString);
                }

                builder.AddOllamaChatCompletion(
                    modelId: parameters.OllamaModelId,
                    httpClient: ollamaHttpClient,
                    serviceId: "ollama");
                break;

            case "openai":
                var openAiHandler = new HttpClientHandler();
                if (parameters.IgnoreSslErrors && !OperatingSystem.IsBrowser())
                {
                    openAiHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                }

                var openAiHttpClient = new HttpClient(new LoggingHttpMessageHandler(parameters.LogStore, openAiHandler));
                builder.AddOpenAIChatCompletion(
                    modelId: parameters.OpenAiModelId,
                    apiKey: parameters.OpenAIApiKey,
                    httpClient: openAiHttpClient);
                break;
        }

#pragma warning restore SKEXP0070

        builder.Services.AddSingleton<LogStore>(parameters.LogStore);
        builder.Services.AddSingleton<ILoggerProvider, LogViewLoggerProvider>();

        builder.Services.AddLogging(logging =>
        {
            logging.ClearProviders();
            // Directly use the passed logStore instance.
            logging.AddProvider(new LogViewLoggerProvider(parameters.LogStore));
            logging.SetMinimumLevel(LogLevel.Trace);
        });

        if (parameters.IncludePlugins)
        {
            builder.Services.AddSingleton(parameters.Maze);
            builder.Plugins.AddFromType<TimeInformationPlugin>();

            var mazeRunnerPlugin = new MazeRunnerRobotPlugin(parameters);
            builder.Plugins.AddFromObject(mazeRunnerPlugin);
        }
        var kernel = builder.Build();
        return kernel;
    }
}