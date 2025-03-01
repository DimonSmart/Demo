using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.Text.Json;
using System.ComponentModel;
using DimonSmart.AiUtils;
using System.Text.Json.Serialization;
using Azure;

namespace Demo.Demos.MazeRunner
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Command
    {
        MoveLeft,
        MoveRight,
        MoveUp,
        MoveDown,
        Done
    }

    public class CommandResult
    {
        public Command command { get; set; }
    }

    public class MazeRunnerRobotPlugin
    {
        private readonly MazeRunnerMaze _maze;
        private readonly string _modelId;

        public MazeRunnerRobotPlugin(MazeRunnerMaze maze, string modelId)
        {
            _maze = maze;
            _modelId = modelId;
        }

        [KernelFunction("PlanRobotAction")]
        [Description("Handle user request commands to the robot to take action in a maze. Return full robot movement history.")]
        public async Task<string> PlanRobotAction(
            [Description("The full text of the user's request containing instructions for the robot.")]
            string robotRequestFullText)
        {
            var userInput = robotRequestFullText;
            var chatHistory = new ChatHistory();

            var kernel = KernelFactory.BuildKernel(new KernelBuildParameters(_maze, _modelId, false));
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            var discoveredMazeView = _maze.MakeMazeAsTextRepresentation();

            var prompt = $@"
Robot Movement and Action Planner:
The robot is navigating a maze and supports only the following commands: MoveLeft, MoveRight, MoveUp, MoveDown, Stop.
Below is the current state of the maze:
{discoveredMazeView}

Maze Description:
- The maze is a grid with rows and columns.
- Each cell is described as:
    • 'Robot' if the cell contains the robot,
    • 'Wall' if it's a wall,
    • 'Apple' or 'Pear' if it contains fruits,
    • 'Empty' if the cell is empty,
    • '?' if undiscovered.
- Column headers and row numbers are provided for navigation.

Instructions:
1. Analyze the user input.
2. If input is conversational, return the JSON {{""Command"": ""Stop""}}.
3. If input instructs an action, choose the appropriate command.
4. If action is not possible, return {{""Command"": ""Stop""}}.
5. IMPORTANT: Return ONLY a valid JSON with a single field ""Command"". No extra text.
";

            chatHistory.AddAssistantMessage(prompt);
            chatHistory.AddAssistantMessage($"Initial maze view: {{{_maze.MakeMazeAsTextRepresentation()}}}");
            chatHistory.AddUserMessage(userInput);

            while (true)
            {
#pragma warning disable SKEXP0070
                var chatMessage = await chatCompletionService.GetChatMessageContentAsync(
                    chatHistory,
                    new OllamaPromptExecutionSettings
                    {
                        Temperature = 0.0f,
                        FunctionChoiceBehavior = FunctionChoiceBehavior.None()
                    },
                    kernel);
#pragma warning restore SKEXP0070

                string responseLine = chatMessage.Content!;
                if (string.IsNullOrWhiteSpace(responseLine))
                {
                    return "Done";
                }

                var cleanedJson = JsonExtractor.ExtractJson(responseLine);
                if (string.IsNullOrWhiteSpace(cleanedJson))
                {
                    chatHistory.AddAssistantMessage("Invalid JSON format received. Expected format: {\"Command\": \"...\"}.");
                    continue;
                }

                CommandResult commandResult;
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    commandResult = JsonSerializer.Deserialize<CommandResult>(cleanedJson, options);
                }
                catch (Exception ex)
                {
                    chatHistory.AddAssistantMessage($"JSON parsing error: {ex.Message}");
                    continue;
                }

                if (commandResult == null || !Enum.IsDefined(typeof(Command), commandResult.command))
                {
                    chatHistory.AddAssistantMessage($"Invalid command received in JSON: {cleanedJson}");
                    continue;
                }

                var command = commandResult.command;
                var executionStatus = string.Empty;
                switch (command)
                {
                    case Command.MoveUp:
                        executionStatus = _maze.Robot.MoveUp();
                        break;
                    case Command.MoveDown:
                        executionStatus = _maze.Robot.MoveDown();
                        break;
                    case Command.MoveLeft:
                        executionStatus = _maze.Robot.MoveLeft();
                        break;
                    case Command.MoveRight:
                        executionStatus = _maze.Robot.MoveRight();
                        break;
                    case Command.Done:
                    default:
                        executionStatus = "Done";
                        break;
                }

                chatHistory.AddAssistantMessage($"Command '{command}' execution status: {executionStatus}");
                chatHistory.AddAssistantMessage($"Updated maze view: {{{_maze.MakeMazeAsTextRepresentation()}}}");

                if (executionStatus == "Stop")
                {
                    return executionStatus;
                }
            }
        }
    }
}
