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

        var loggingHandler = new LoggingHttpMessageHandler(parameters.LogStore);

        switch (parameters.connectionType.ToLower())
        {
            case "ollama":
                var ollamaHttpClient = new HttpClient(loggingHandler)
                {
                    BaseAddress = new Uri("http://localhost:11434"),
                    Timeout = TimeSpan.FromMinutes(20)
                };
                builder.AddOllamaChatCompletion(
                    modelId: parameters.OllamaModelId,
                    httpClient: ollamaHttpClient,
                    serviceId: "ollama");
                break;

            case "openai":
                var openAiHttpClient = new HttpClient(loggingHandler);
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