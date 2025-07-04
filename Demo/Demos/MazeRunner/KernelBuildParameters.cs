namespace Demo.Demos.MazeRunner
{
    public record KernelBuildParameters(MazeRunnerMaze Maze, string connectionType, string OllamaModelId, string OpenAiModelId, bool IncludePlugins, string OpenAIApiKey, LogStore LogStore, string OllamaServerUrl, string OllamaPassword, Func<Task>? OnStateChangedAsync = null);
}