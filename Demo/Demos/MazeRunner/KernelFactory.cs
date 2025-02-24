using Microsoft.SemanticKernel;

namespace Demo.Demos.MazeRunner
{
    public static class KernelFactory
    {
        public static Kernel BuildKernel(MazeRunnerMaze maze)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            var builder = Kernel.CreateBuilder();

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:11434"),
                Timeout = TimeSpan.FromMinutes(20)
            };

#pragma warning disable SKEXP0070
            builder.AddOllamaChatCompletion(
                modelId: "llama3.3:70b-instruct-q2_K",
                httpClient: httpClient,
                serviceId: "ollama"
            );
#pragma warning restore SKEXP0070

            builder.Services.AddSingleton<LogStore>();
            builder.Services.AddSingleton<ILoggerProvider, LogViewLoggerProvider>();

            builder.Services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Trace);
            });

            builder.Services.AddSingleton(maze);
            builder.Plugins.AddFromType<TimeInformationPlugin>();

            var kernel = builder.Build();
            return kernel;
        }
    }
}