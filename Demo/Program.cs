using Blazored.LocalStorage;
using Demo;
using Demo.Abstractions;
using Demo.Demos.Common;
using Demo.Demos.HashX;
using Demo.Demos.MazeRunner;
using Demo.Demos.Quiz;
using Demo.Services;
using Demo.Core.Quiz;
using DimonSmart.Hash.Interfaces;
using KristofferStrube.Blazor.MediaCaptureStreams;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Options;
using System.Text;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddBlazoredLocalStorageAsSingleton(options =>
{
    // Switched off NullabilityInfoContext, to workaround the issue with Blazored.LocalStorage
    options.JsonSerializerOptions.TypeInfoResolver = null;
});

builder.Services.AddSingleton<DemoPageChromeService>();
builder.Services.AddSingleton<IDemoPageChromeService>(sp => sp.GetRequiredService<DemoPageChromeService>());
builder.Services.AddSingleton<LogStore>();
builder.Services.AddScoped<BrowserService>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMediaDevicesService();

RegisterHashDemoServices(builder.Services);
RegisterQuizDemoServices(builder.Services);
RegisterMazeRunnerDemoServices(builder);

await builder.Build().RunAsync();

static void RegisterHashDemoServices(IServiceCollection services)
{
    services.AddScoped<IHashAlgorithm, JsMd5Algorithm>();
}

static void RegisterQuizDemoServices(IServiceCollection services)
{
    services.AddScoped<CardStorageService>();
    services.AddScoped<IQuizCatalogService, QuizCatalogService>();
    services.AddScoped<IQuizSourceLoader, QuizSourceLoader>();
    services.AddScoped<QuizProgressService>();
    services.AddScoped<IQuizSessionService, QuizSessionService>();
    services.AddScoped<QuizPreferencesService>();
    services.AddScoped<QuizSelectionService>();
    services.AddScoped<TextTranslationService>();
    services.AddScoped<IQuizDocumentLoader, QuizDocumentLoader>();
    services.AddScoped<IQuizDataService, QuizDataService>();
    services.AddScoped<IQuizStatisticsService, QuizStatisticsService>();
    services.AddScoped<IQuizLanguageService, QuizLanguageService>();
}

static void RegisterMazeRunnerDemoServices(WebAssemblyHostBuilder hostBuilder)
{
    hostBuilder.Services.Configure<OllamaOptions>(options =>
    {
        options.BaseAddress = "http://localhost:11434";
    });

    hostBuilder.Services.AddHttpClient();
    hostBuilder.Services.AddScoped<IOllamaConfigurationProvider, OllamaConfigurationProvider>();
    hostBuilder.Services.AddScoped<IOllamaHttpClientProvider, OllamaHttpClientProvider>();
    hostBuilder.Services.AddScoped<IOllamaModelService, OllamaModelService>();
    hostBuilder.Services.AddScoped<UserPreferencesStorageService<MazeRunnerUserPreferences>>();
}
