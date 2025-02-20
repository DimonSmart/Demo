using Microsoft.SemanticKernel;

namespace Demo.Demos.MazeRunner
{
    public static class KernelFactory
    {
        public static Kernel BuildKernel(ILoggerFactory loggerFactory)
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

            // Register the logger factory and logging
            builder.Services.AddSingleton(loggerFactory);
            builder.Services.AddLogging(logging =>
            {
                logging.ClearProviders();
                //  logging.AddProvider(new RegionLoggerProvider());
                logging.SetMinimumLevel(LogLevel.Trace);
            });

            var kernel = builder.Build();
            return kernel;
        }
    }
}