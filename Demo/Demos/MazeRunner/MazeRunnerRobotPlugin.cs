using Microsoft.SemanticKernel;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Demo.Demos.MazeRunner
{
    public class MazeRunnerRobotPlugin
    {
        private readonly MazeRunnerMaze _maze;
        private readonly KernelBuildParameters _kernelBuildParameters;

        public MazeRunnerRobotPlugin(MazeRunnerMaze maze, KernelBuildParameters kernelBuildParameters)
        {
            _kernelBuildParameters = kernelBuildParameters;
            _maze = maze;
        }

        [KernelFunction("MoveUp")]
        [Description("Move the robot up one cell (Up: (X, Y-1)). Returns 'Ok' when successful, 'Err' when blocked, and appends the current robot coordinates (X,Y).")]
        public Task<string> MoveUp() => ExecuteMoveAsync(_maze.Robot.MoveUp, "MoveUp");

        [KernelFunction("MoveDown")]
        [Description("Move the robot down one cell (Down: (X, Y+1)). Returns 'Ok' when successful, 'Err' when blocked, and appends the current robot coordinates (X,Y).")]
        public Task<string> MoveDown() => ExecuteMoveAsync(_maze.Robot.MoveDown, "MoveDown");

        [KernelFunction("MoveLeft")]
        [Description("Move the robot left one cell (Left: (X-1, Y)). Returns 'Ok' when successful, 'Err' when blocked, and appends the current robot coordinates (X,Y).")]
        public Task<string> MoveLeft() => ExecuteMoveAsync(_maze.Robot.MoveLeft, "MoveLeft");

        [KernelFunction("MoveRight")]
        [Description("Move the robot right one cell (Right: (X+1, Y)). Returns 'Ok' when successful, 'Err' when blocked, and appends the current robot coordinates (X,Y).")]
        public Task<string> MoveRight() => ExecuteMoveAsync(_maze.Robot.MoveRight, "MoveRight");

        [KernelFunction("ReportGoalReached")]
        [Description("Call this when the robot has accomplished the goal. Provide a short summary of the work using the 'summary' parameter.")]
        public async Task<string> ReportGoalReached(string summary = "")
        {
            _maze.GoalAchieved = true;
            var trimmedSummary = summary?.Trim();
            var message = string.IsNullOrWhiteSpace(trimmedSummary)
                ? "ReportGoalReached => Goal reported as complete"
                : $"ReportGoalReached => Goal reported as complete. Summary: {trimmedSummary}";
            _kernelBuildParameters.LogStore.Messages.Add(new LogStore.LogMessage(message, LogStore.LogType.RobotMovements));
            _maze.RecordCommand(message);
            if (_kernelBuildParameters.OnStateChangedAsync != null)
            {
                await _kernelBuildParameters.OnStateChangedAsync();
            }

            return string.IsNullOrWhiteSpace(trimmedSummary)
                ? "Goal reported as complete."
                : $"Goal reported as complete. Summary noted: {trimmedSummary}";
        }

        private async Task<string> ExecuteMoveAsync(Func<string> move, string commandName)
        {
            var result = move();
            var message = $"{commandName} => {result}. Robot position: x={_maze.Robot.X}, y={_maze.Robot.Y}";
            _kernelBuildParameters.LogStore.Messages.Add(new LogStore.LogMessage(message, LogStore.LogType.RobotMovements));
            _maze.RecordCommand(message);
            if (_kernelBuildParameters.OnStateChangedAsync != null)
            {
                await _kernelBuildParameters.OnStateChangedAsync();
            }

            return $"{result}. Robot position: x={_maze.Robot.X}, y={_maze.Robot.Y}";
        }
    }
}
