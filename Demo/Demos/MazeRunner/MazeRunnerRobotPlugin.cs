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

        // Keep track of all steps performed so far:
        private readonly List<string> _robotMoves = new();

        // Optionally, store the initial maze representation (to show each time if you wish).
        private readonly string _initialMazeView;

        public MazeRunnerRobotPlugin(MazeRunnerMaze maze, string modelId)
        {
            _maze = maze;
            _modelId = modelId;

            // Capture the starting layout of the maze if needed:
            _initialMazeView = _maze.MakeMazeAsTextRepresentation();
        }

        [KernelFunction("PlanRobotAction")]
        [Description("Handle user request commands to the robot to take action in a maze. Return full robot movement history.")]
        public async Task<string> PlanRobotAction(
            [Description("The **full** text of the user's request containing instructions and conditions for controlling the robot.")]
            string robotMovementRequestFullText)
        {
            var kernel = KernelFactory.BuildKernel(new KernelBuildParameters(_maze, _modelId, IncludePlugins: false));
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            while (true)
            {
                // Build a fresh ChatHistory each iteration
                var chatHistory = new ChatHistory();

                // Current maze as text
                var currentMazeView = _maze.MakeMazeAsTextRepresentation();
                // Steps so far, to show LLM
                var movesSoFar = string.Join("\n", _robotMoves);

                var canUp = _maze.Robot.CanMoveUp();
                var canDown = _maze.Robot.CanMoveDown();
                var canLeft = _maze.Robot.CanMoveLeft();
                var canRight = _maze.Robot.CanMoveRight();

                var systemPrompt = $@"
MAZE DETAILS:
Symbols:
- '#' means a wall (not passable).
- 'R' means the robot's current position.
- '.' means a discovered free cell.
- '?' means an undiscovered cell (robot should be 1 cell close to discover).

Current Maze State:
""{currentMazeView}""

Moves so far:
{(string.IsNullOrWhiteSpace(movesSoFar) ? "(none yet)" : movesSoFar)}

USER REQUEST:
""{robotMovementRequestFullText}""

INSTRUCTIONS:
1. Analyze the user's request and ""Possible Moves"" and the ""Current Maze State"" **carefully**.
2. ""USER REQUEST"" is just a recommendation. You have to check if it's possible to follow it.
2. Propose exactly ONE next move: {(canUp ? "MoveUp\n" : "")}{(canDown ? "MoveDown\n" : "")}{(canLeft ? "MoveLeft\n" : "")}{(canRight ? "MoveRight\n" : "")}.
3. If direction is blocked by wall or the ""USER REQUEST"" is fulfilled, return ""Done"".
4. Your response MUST be valid JSON with exactly one field ""command"". 
   Example: {{""command"": ""MoveRight""}} or {{""command"": ""Done""}}.
";

                // Add system prompt to the chat
                chatHistory.AddSystemMessage(systemPrompt);
                // Add user request as user message (just to keep context, though systemPrompt is key)
                chatHistory.AddUserMessage(robotMovementRequestFullText);

#pragma warning disable SKEXP0070
                var chatMessage = await chatCompletionService.GetChatMessageContentAsync(
                    chatHistory,
                    new OllamaPromptExecutionSettings
                    {
                        Temperature = 0.1f,
                        FunctionChoiceBehavior = FunctionChoiceBehavior.None()
                    },
                    kernel);
#pragma warning restore SKEXP0070

                var responseLine = chatMessage.Content ?? string.Empty;
                if (string.IsNullOrWhiteSpace(responseLine))
                {
                    return "No response from model; stopping.";
                }

                var cleanedJson = JsonExtractor.ExtractJson(responseLine);
                if (string.IsNullOrWhiteSpace(cleanedJson))
                {
                    return "No valid JSON found in the model's response; stopping.";
                }

                // Deserialize the JSON to get the Command
                CommandResult commandResult;
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    commandResult = JsonSerializer.Deserialize<CommandResult>(cleanedJson, options)
                                   ?? new CommandResult { command = Command.Done };
                }
                catch
                {
                    return "Error deserializing JSON; stopping.";
                }

                // Check the command
                if (!Enum.IsDefined(typeof(Command), commandResult.command))
                {
                    return $"Invalid command returned: {commandResult.command}";
                }

                var command = commandResult.command;
                if (command == Command.Done)
                {
                    // Return all performed steps as final output
                    return $"All steps done. Moves so far:\n - {string.Join("\n - ", _robotMoves)}";
                }

                var status = command switch
                {
                    Command.MoveUp => _maze.Robot.MoveUp(),
                    Command.MoveDown => _maze.Robot.MoveDown(),
                    Command.MoveLeft => _maze.Robot.MoveLeft(),
                    Command.MoveRight => _maze.Robot.MoveRight(),
                    _ => "Unknown"
                };

                // Append the executed step to our log
                _robotMoves.Add($"Command {command} => {status}");
                if (status == "Err") return $"All steps done. Moves so far:\n - {string.Join("\n - ", _robotMoves)}"; ;
            }
        }
    }
}
