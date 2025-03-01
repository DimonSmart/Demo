using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.ComponentModel;

namespace Demo.Demos.MazeRunner
{
    public enum Command
    {
        MoveLeft,
        MoveRight,
        MoveUp,
        MoveDown,
        LookAround,
        Stop
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
            // Get user input and initialize chat history.
            var userInput = robotRequestFullText;
            var chatHistory = new ChatHistory();

            // Build a kernel instance using the provided maze and model id.
            var kernel = KernelFactory.BuildKernel(new KernelBuildParameters(_maze, _modelId, false));
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            // Retrieve the current discovered/open part of the maze for context.
            var discoveredMazeView = _maze.MakeMazeAsTextRepresentation();


            var prompt = $@"
Robot Movement and Action Planner:
The robot is navigating a maze and supports only the following commands: MoveLeft, MoveRight, MoveUp, MoveDown, Stop.
Below is the current state of the maze:
{discoveredMazeView}

Maze Description:
- The maze is represented as a grid with rows and columns.
- Each cell is described as follows:
    • 'Robot' if the cell contains the robot,
    • 'Wall' if it's a wall,
    • 'Apple' or 'Pear' if it contains fruits,
    • 'Empty' if the cell is empty,
    • '?' if the cell is not discovered.
- Column headers and row numbers are provided for easy navigation.

Instructions:
1. Analyze the user input.
2. If the input is just conversational, return the JSON {{""Command"": ""Stop""}}.
3.If the input instructs the robot to perform an action, choose the appropriate command from the allowed list.
4.If the specified action is not possible, return {{""Command"": ""Stop""}}.
5.IMPORTANT: Return ONLY a valid JSON with a single field ""Command"". Do not include any extra text, commentary, or debugging information.
";


            chatHistory.AddAssistantMessage(prompt);
            chatHistory.AddAssistantMessage($"Initial maze view: {{{_maze.MakeMazeAsTextRepresentation()}}}");
            chatHistory.AddUserMessage(userInput);

            while (true)
            {
#pragma warning disable SKEXP0070
                var result = await chatCompletionService.GetChatMessageContentAsync(
                    chatHistory,
                    new OllamaPromptExecutionSettings
                    {
                        Temperature = 0.0f,
                        FunctionChoiceBehavior = FunctionChoiceBehavior.None()
                    },
                    kernel);
#pragma warning restore SKEXP0070
                var responseLine = result.ToString().Trim();
                if (string.IsNullOrWhiteSpace(responseLine))
                {
                    return "Stop";
                }

                if (Enum.TryParse<Command>(responseLine, true, out var command))
                {
                    var executionStatus = string.Empty;

                    // Execute the corresponding robot action and capture the status.
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
                        case Command.Stop:
                        default:
                            executionStatus = "Stop";
                            break;
                    }

                    // Add the command execution status to the chat history.
                    chatHistory.AddAssistantMessage($"Command '{command}' execution status: {executionStatus}");
                    chatHistory.AddAssistantMessage($"Updated maze view: {{{_maze.MakeMazeAsTextRepresentation()}}}");

                    // If the command is a Stop command, break the loop.
                    if (executionStatus == "Stop")
                    {
                        return executionStatus;
                    }
                }
                else
                {
                    // If the response cannot be parsed into a valid command, notify via chat history.
                    chatHistory.AddAssistantMessage(
                        $"Invalid command received: '{responseLine}'. Expected one of: MoveLeft, MoveRight, MoveUp, MoveDown, Stop.");
                }
            }
        }
    }
}
