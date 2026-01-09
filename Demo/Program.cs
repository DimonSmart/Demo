using Blazored.LocalStorage;
using BlazorImageProcessing;
using Demo;
using Demo.Abstractions;
using Demo.Demos.Common;
using Demo.Demos.HashX;
using Demo.Demos.MazeRunner;
using Demo.Demos.Pdd;
using Demo.Services;
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

builder.Services.AddSingleton<PageTitleService>();
builder.Services.AddSingleton<IPageTitleService>(sp => sp.GetRequiredService<PageTitleService>());
builder.Services.AddSingleton<LogStore>();
builder.Services.AddScoped<BrowserService>();
builder.Services.AddScoped<ImageProcessingService>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMediaDevicesService();

RegisterHashDemoServices(builder.Services);
RegisterPddDemoServices(builder.Services);
RegisterMazeRunnerDemoServices(builder);

await builder.Build().RunAsync();

static void RegisterHashDemoServices(IServiceCollection services)
{
    services.AddScoped<IHashAlgorithm, JsMd5Algorithm>();
}

static void RegisterPddDemoServices(IServiceCollection services)
{
    services.AddScoped<CardStorageService>();
    services.AddScoped<UserPreferencesStorageService<PddUserPreferences>>();
    services.AddScoped<TextTranslationService>();
    services.AddScoped<IPddDataService, PddDataService>();
    services.AddScoped<IPddStatisticsService, PddStatisticsService>();
    services.AddScoped<IPddLanguageService, PddLanguageService>();
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
