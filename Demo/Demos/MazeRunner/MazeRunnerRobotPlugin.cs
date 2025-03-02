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
            [Description("The **full** text of the user's request containing instructions and conditions for control the robot.")]
            string robotMovementRequestFullText)
        {
            var kernel = KernelFactory.BuildKernel(new KernelBuildParameters(_maze, _modelId, IncludePlugins: false));
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            // We process user commands in a loop until the user or the model decides we are 'Done'.
            while (true)
            {
                // Build a fresh ChatHistory each iteration to avoid an ever-growing conversation:
                var chatHistory = new ChatHistory();

                // Show the initial maze once if you want; then the steps so far; then the current maze.
                // Because we build it each iteration, the user sees a "reset" prompt that includes only
                // the relevant data, not a giant repeated conversation.
                var currentMazeView = _maze.MakeMazeAsTextRepresentation();

                // If you want to store the entire sequence of moves as lines:
                var movesSoFar = string.Join("\n", _robotMoves);

                var systemPrompt = $@"
MAZE DETAILS:
Symbols:
- '#' means a wall that the robot cannot pass.
- 'R' means the robot's current location.
- '.' means a discovered free cell.
- '?' means an undiscovered cell.

Coordinate System:
- The maze uses an (x, y) coordinate system. (x - columns, y - rows)
- Increasing x corresponds right direction.
- Increasing y corresponds down direction.

Initial Maze (starting point):
""{_initialMazeView}""

Steps Performed:
{(string.IsNullOrWhiteSpace(movesSoFar) ? "(none yet)" : movesSoFar)}

Current Maze State:
""{currentMazeView}""

User Request:
""{robotMovementRequestFullText}""

INSTRUCTIONS:
0. Try to plan next robot movement command based on user request and current maze state.
1. If the user is just chatting or you cannot understand the user request, return command ""Done"".
2. Check if the list of 'Steps Performed' satisfy user request then return command ""Done"".
3. If you can not find next robot movement command, return command ""Done"".
4. **The robot cannot pass through walls**: if the next position is `'#'`, do not attempt to move there. If the user asked for more steps but the next cell is blocked, treat the request as completed and return ""Done"".
5. Decide which single command (MoveLeft, MoveRight, MoveUp, MoveDown) should be performed next to satisfy user input.
6. You can **only** return a valid JSON with a single ""command"" field:
  - MoveLeft, MoveRight, MoveUp, MoveDown, or Done. Example: {{""command"": ""MoveLeft""}}.
  Do not add extra text or reasoning outside that JSON.
";
                // Add system prompt to the chat
                chatHistory.AddSystemMessage(systemPrompt);
                chatHistory.AddUserMessage(robotMovementRequestFullText);

                // Ask the LLM for the single-line JSON output
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
                    // If we somehow got no content, we can break or just treat as "done".
                    return "No response from model; stopping.";
                }

                // Extract JSON from the response.
                var cleanedJson = JsonExtractor.ExtractJson(responseLine);
                if (string.IsNullOrWhiteSpace(cleanedJson))
                {
                    // If there's no valid JSON, we can treat as a "done" or ask again.
                    // But let's just break for this example:
                    return "No valid JSON found; stopping.";
                }

                // Deserialize the JSON to get the Command
                CommandResult commandResult;
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
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
                    // Return all performed steps as the final "history," if you like.
                    return $"All steps done. Moves so far:\n - {string.Join("\n - ", _robotMoves)}";
                }

                // Otherwise, execute the command
                var status = command switch
                {
                    Command.MoveUp => _maze.Robot.MoveUp(),
                    Command.MoveDown => _maze.Robot.MoveDown(),
                    Command.MoveLeft => _maze.Robot.MoveLeft(),
                    Command.MoveRight => _maze.Robot.MoveRight(),
                    _ => "Unknown"
                };

                // Append the executed step to our in-code list
                _robotMoves.Add($"Command {command} => {status}");

                // If for some reason your Move methods can return a "Stop," break out
                if (command ==  Command.Done)
                {
                    return $"Robot indicated 'Done'. Moves so far:\n - {string.Join("\n - ", _robotMoves)}";
                }
            }
        }
    }
}
