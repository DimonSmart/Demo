﻿using System.Text.Json;

namespace Demo.Demos.MazeRunner
{
    public class OllamaModelService(IOllamaHttpClientProvider httpClientProvider) : IOllamaModelService
    {

        /// <summary>
        /// Gets a list of loaded models from Ollama.
        /// Returns only ModelId, ParameterCount, and Size.
        /// </summary>
        public async Task<IReadOnlyList<OllamaModelInfo>> GetLoadedModelsAsync()
        {
            try
            {
                using var httpClient = await httpClientProvider.CreateConfiguredHttpClientAsync();
                var response = await httpClient.GetAsync("/api/tags");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var modelsResponse = JsonSerializer.Deserialize<LoadedModelsResponse>(
                    json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var result = new List<OllamaModelInfo>();
                if (modelsResponse?.Models != null)
                {
                    foreach (var model in modelsResponse.Models)
                    {
                        result.Add(new OllamaModelInfo
                        {
                            ModelId = model.name,
                            ParameterCount = model.details?.parameter_size ?? string.Empty,
                            Size = model.size
                        });
                    }
                }

                return result;
            }
            catch
            {
                return new List<OllamaModelInfo>();
            }
        }
    }
}