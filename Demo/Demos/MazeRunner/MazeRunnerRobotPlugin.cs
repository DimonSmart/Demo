using DimonSmart.AiUtils;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Demo.Demos.MazeRunner
{
    public class MazeRunnerRobotPlugin
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum NextStepRobotCommand
        {
            MoveLeft,
            MoveRight,
            MoveUp,
            MoveDown,
            Done
        }

        public class CommandResult
        {
            public NextStepRobotCommand command { get; set; }
        }

        public class RobotState
        {
            public int CurrentX { get; set; }
            public int CurrentY { get; set; }
            public HashSet<(int x, int y)> ExploredCells { get; set; } = new();
            public string CurrentGoal { get; set; } = string.Empty;
        }

        private readonly MazeRunnerMaze _maze;

        private readonly List<string> _robotMoves = new();

        private readonly KernelBuildParameters _kernelBuildParameters;

        private readonly RobotState _robotState = new();

        public MazeRunnerRobotPlugin(KernelBuildParameters kernelBuildParameters)
        {
            _maze = kernelBuildParameters.Maze;
            _kernelBuildParameters = kernelBuildParameters;
        }

     //   [KernelFunction("PlanRobotAction")]
     //   [Description("Handle user request commands to the robot to take action in a maze. Return full robot movement history.")]
        public async Task<string> PlanRobotAction(
            [Description("The **full** text of the user's request containing instructions and conditions for controlling the robot.")]
            string robotMovementRequestFullText)
        {
            var kernel = KernelFactory.BuildKernel(_kernelBuildParameters with { IncludePlugins = false });
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            while (true)
            {
                var chatHistory = new ChatHistory();
                var currentMazeView = _maze.MakeMazeAsSimpleCellList();
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

                chatHistory.AddSystemMessage(systemPrompt);
                chatHistory.AddUserMessage(robotMovementRequestFullText);

#pragma warning disable SKEXP0070
                var settings = _kernelBuildParameters.connectionType.ToLower() == "ollama" 
                    ? new OllamaPromptExecutionSettings { Temperature = 0.1f, FunctionChoiceBehavior = FunctionChoiceBehavior.None() }
                    : new PromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.None() };
#pragma warning restore SKEXP0070

                var chatMessage = await chatCompletionService.GetChatMessageContentAsync(
                    chatHistory,
                    settings,
                    kernel);

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

                CommandResult commandResult;
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    commandResult = JsonSerializer.Deserialize<CommandResult>(cleanedJson, options)
                                   ?? new CommandResult { command = NextStepRobotCommand.Done };
                }
                catch
                {
                    return "Error deserializing JSON; stopping.";
                }

                if (!Enum.IsDefined(typeof(NextStepRobotCommand), commandResult.command))
                {
                    return $"Invalid command returned: {commandResult.command}";
                }

                var command = commandResult.command;
                if (command == NextStepRobotCommand.Done)
                {
                    return $"All steps done. Moves so far:\n - {string.Join("\n - ", _robotMoves)}";
                }

                var status = command switch
                {
                    NextStepRobotCommand.MoveUp => _maze.Robot.MoveUp(),
                    NextStepRobotCommand.MoveDown => _maze.Robot.MoveDown(),
                    NextStepRobotCommand.MoveLeft => _maze.Robot.MoveLeft(),
                    NextStepRobotCommand.MoveRight => _maze.Robot.MoveRight(),
                    _ => "Unknown"
                };

                _robotMoves.Add($"Command {command} => {status}");
                if (status == "Err") return $"All steps done. Moves so far:\n - {string.Join("\n - ", _robotMoves)}";
            }
        }

        [KernelFunction("PlanSmartRobotAction")]
        [Description("Handle goal-oriented robot movement commands in the maze")]
        public async Task<string> PlanSmartRobotAction(
            [Description("The user's goal for the robot (e.g., 'explore right side', 'go to bottom', 'explore whole maze')")]
            string userGoal)
        {
            var kernel = KernelFactory.BuildKernel(_kernelBuildParameters with { IncludePlugins = false });
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            _robotState.CurrentGoal = userGoal;

            while (true)
            {
                var chatHistory = new ChatHistory();
                var currentMazeView = _maze.MakeMazeAsTextRepresentation();
                //.MakeMazeAsSimpleCellList();
                var movesSoFar = string.Join("\n", _robotMoves);
                
                // Get current position and possible moves
                var canUp = _maze.Robot.CanMoveUp();
                var canDown = _maze.Robot.CanMoveDown();
                var canLeft = _maze.Robot.CanMoveLeft();
                var canRight = _maze.Robot.CanMoveRight();

                var systemPrompt = $@"
You are an intelligent robot navigation system. Your task is to guide a robot through a maze to achieve a specific goal.

MAZE DETAILS:
Current layout:
{currentMazeView}

Legend:
- '#' = Wall (impassable)
- 'R' = Robot's current position
- '.' = Discovered free cell
- '?' = Undiscovered cell (requires robot to be adjacent to discover)

CURRENT STATE:
- Goal: {_robotState.CurrentGoal}
- Previous moves: {(string.IsNullOrWhiteSpace(movesSoFar) ? "None" : movesSoFar)}

AVAILABLE MOVES:
{(canUp ? "- MoveUp (↑)\n" : "")}{(canDown ? "- MoveDown (↓)\n" : "")}{(canLeft ? "- MoveLeft (←)\n" : "")}{(canRight ? "- MoveRight (→)\n" : "")}

INSTRUCTIONS:
1. Analyze the current state and compare it to the goal: ""{userGoal}""
2. Consider:
   - Which unexplored areas are relevant to the goal?
   - What's the most efficient next move toward the goal?
   - Have we achieved the goal?
3. Return ONE of these commands as JSON:
   - Movement: {{""command"": ""MoveUp/Down/Left/Right""}}
   - Goal complete: {{""command"": ""Done""}}

Your response must be valid JSON with exactly one ""command"" field.
Think strategically about achieving the goal efficiently.
";

                chatHistory.AddSystemMessage(systemPrompt);
                chatHistory.AddUserMessage($"Based on the current state and goal: {userGoal}, what should be the next move?");

#pragma warning disable SKEXP0070
                var settings = _kernelBuildParameters.connectionType.ToLower() == "ollama" 
                    ? new OllamaPromptExecutionSettings { Temperature = 0.1f, FunctionChoiceBehavior = FunctionChoiceBehavior.None() }
                    : new PromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.None() };
#pragma warning restore SKEXP0070

                var chatMessage = await chatCompletionService.GetChatMessageContentAsync(
                    chatHistory,
                    settings,
                    kernel);

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

                CommandResult commandResult;
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    commandResult = JsonSerializer.Deserialize<CommandResult>(cleanedJson, options)
                                   ?? new CommandResult { command = NextStepRobotCommand.Done };
                }
                catch
                {
                    return "Error deserializing JSON; stopping.";
                }

                if (!Enum.IsDefined(typeof(NextStepRobotCommand), commandResult.command))
                {
                    return $"Invalid command returned: {commandResult.command}";
                }

                var command = commandResult.command;
                if (command == NextStepRobotCommand.Done)
                {
                    return $"Goal '{userGoal}' completed. Movement history:\n - {string.Join("\n - ", _robotMoves)}";
                }

                var status = command switch
                {
                    NextStepRobotCommand.MoveUp => _maze.Robot.MoveUp(),
                    NextStepRobotCommand.MoveDown => _maze.Robot.MoveDown(),
                    NextStepRobotCommand.MoveLeft => _maze.Robot.MoveLeft(),
                    NextStepRobotCommand.MoveRight => _maze.Robot.MoveRight(),
                    _ => "Unknown"
                };

                if (status == "Ok")
                {
                    _robotMoves.Add($"Command {command} => Success");
                    // Update explored cells set
                    var (x, y) = (_maze.Robot.X, _maze.Robot.Y);
                    _robotState.ExploredCells.Add((x, y));
                    _robotState.CurrentX = x;
                    _robotState.CurrentY = y;
                }
                else
                {
                    _robotMoves.Add($"Command {command} => Failed");
                    return $"Movement failed. Goal '{userGoal}' aborted. Movement history:\n - {string.Join("\n - ", _robotMoves)}";
                }
            }
        }
    }
}
