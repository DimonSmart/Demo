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
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Options;

// Existing code
var builder = WebAssemblyHostBuilder.CreateDefault(args);
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

// Hash demo
builder.Services.AddScoped<IHashAlgorithm, JsMd5Algorithm>();

// Pdd demo
builder.Services.AddScoped<CardStorageService>();
builder.Services.AddScoped<UserPreferencesStorageService<PddUserPreferences>>();
builder.Services.AddScoped<TextTranslationService>();
builder.Services.AddScoped<IPddDataService, PddDataService>();
builder.Services.AddScoped<IPddStatisticsService, PddStatisticsService>();
builder.Services.AddScoped<IPddLanguageService, PddLanguageService>();

// MazeRunner demo
builder.Services.Configure<OllamaOptions>(options =>
{
    options.BaseAddress = "http://localhost:11434";
});
builder.Services.AddHttpClient();
builder.Services.AddScoped<IOllamaConfigurationProvider, OllamaConfigurationProvider>();
builder.Services.AddScoped<IOllamaHttpClientProvider, OllamaHttpClientProvider>();
builder.Services.AddScoped<IOllamaModelService, OllamaModelService>();
builder.Services.AddScoped<UserPreferencesStorageService<MazeRunnerUserPreferences>>();



await builder.Build().RunAsync();
