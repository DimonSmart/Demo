namespace Demo.Demos.MazeRunner;

public class MazeRunnerUserPreferences
{
    public string? SelectedModelId { get; set; } = string.Empty;
    public string? ConnectionType { get; set; } = string.Empty;
    public string? OpenAIApiKey { get; set; }
    public string? OpenAIModel { get; set; }
    public string? OllamaServerUrl { get; set; } = "http://localhost:11434";
    public string? OllamaPassword { get; set; } = string.Empty;
}
