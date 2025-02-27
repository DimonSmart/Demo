using Blazored.LocalStorage;
using BlazorImageProcessing;
using Demo;
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
builder.Services.AddBlazoredLocalStorageAsSingleton();

builder.Services.AddSingleton<PageTitleService>();
builder.Services.AddSingleton<LogStore>();
builder.Services.AddScoped<BrowserService>();
builder.Services.AddScoped<ImageProcessingService>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Hash demo
builder.Services.AddScoped<IHashAlgorithm, JsMd5Algorithm>();

// Pdd demo
builder.Services.AddScoped<CardStorageService>();
builder.Services.AddScoped<UserPreferencesStorageService<PddUserPreferences>>();

// MazeRunner demo
builder.Services.Configure<OllamaOptions>(options =>
{
    options.BaseAddress = "http://localhost:11434";
});
builder.Services.AddHttpClient<IOllamaModelService, OllamaModelService>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseAddress);
});
builder.Services.AddScoped<UserPreferencesStorageService<MazeRunnerUserPreferences>>();



await builder.Build().RunAsync();
