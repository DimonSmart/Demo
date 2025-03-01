using Microsoft.SemanticKernel;

namespace Demo.Demos.MazeRunner
{
    public record KernelBuildParameters(MazeRunnerMaze Maze, string ModelId, bool IncludePlugins);

    public static class KernelFactory
    {
        public static Kernel BuildKernel(KernelBuildParameters parameters)
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
                modelId: parameters.ModelId,
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
            if (parameters.IncludePlugins)
            {
                builder.Services.AddSingleton(parameters.Maze);
                builder.Plugins.AddFromType<TimeInformationPlugin>();

                var mazeRunnerPlugin = new MazeRunnerRobotPlugin(parameters.Maze, parameters.ModelId);
                builder.Plugins.AddFromObject(mazeRunnerPlugin);
            }
            var kernel = builder.Build();
            return kernel;
        }
    }
}