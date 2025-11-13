using System.Threading.Tasks;
using Demo.Demos.MazeRunner;

namespace DemoTests.MazeRunner;

public class MazeRunnerRobotPluginTests
{
    [Fact]
    public async Task MoveRight_UpdatesRobotPositionAndCommandHistory()
    {
        var maze = new MazeRunnerMaze(5, 5);
        var logStore = new LogStore();
        var stateChangeCount = 0;
        var parameters = new KernelBuildParameters(
            maze,
            connectionType: "ollama",
            OllamaModelId: "test-model",
            OpenAiModelId: "test-openai",
            IncludePlugins: true,
            OpenAIApiKey: string.Empty,
            LogStore: logStore,
            OllamaServerUrl: "http://localhost",
            OllamaPassword: string.Empty,
            IgnoreSslErrors: false,
            OnStateChangedAsync: () =>
            {
                stateChangeCount++;
                return Task.CompletedTask;
            });

        var plugin = new MazeRunnerRobotPlugin(maze, parameters);

        var initialState = maze.MakeMazeAsTextRepresentation();
        Assert.Contains("# Robot position: (1,1)", initialState);

        var result = await plugin.MoveRight();

        Assert.Equal("Ok. Robot position: (2, 1)", result);
        Assert.Equal(2, maze.Robot.X);
        Assert.Equal(1, maze.Robot.Y);
        Assert.Contains("MoveRight => Ok. Robot position: (2, 1)", maze.CommandHistory);
        Assert.NotEmpty(logStore.Messages);
        Assert.Equal(1, stateChangeCount);

        var updatedState = maze.MakeMazeAsTextRepresentation();
        Assert.Contains("# Robot position: (2,1)", updatedState);
    }
}
