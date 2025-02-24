using Blazored.LocalStorage;
using BlazorImageProcessing;
using Demo;
using Demo.Demos.HashX;
using Demo.Demos.MazeRunner;
using Demo.Demos.Pdd;
using Demo.Services;
using DimonSmart.Hash.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

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
builder.Services.AddScoped<UserPreferencesStorageService>();

await builder.Build().RunAsync();
