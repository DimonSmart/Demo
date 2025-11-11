using Microsoft.SemanticKernel;
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
        [Description("Move the robot up one cell. Returns 'Ok' when successful, 'Err' when blocked, and appends the current robot coordinates.")]
        public async Task<string> MoveUp()
        {
            var result = _maze.Robot.MoveUp();
            var message = $"MoveUp => {result}. Robot position: ({_maze.Robot.X}, {_maze.Robot.Y})";
            _kernelBuildParameters.LogStore.Messages.Add(new LogStore.LogMessage(message, LogStore.LogType.RobotMovements));
            _maze.RecordCommand(message);
            if (_kernelBuildParameters.OnStateChangedAsync != null)
            {
                await _kernelBuildParameters.OnStateChangedAsync();
            }
            return $"{result}. Robot position: ({_maze.Robot.X}, {_maze.Robot.Y})";
        }

        [KernelFunction("MoveDown")]
        [Description("Move the robot down one cell. Returns 'Ok' when successful, 'Err' when blocked, and appends the current robot coordinates.")]
        public async Task<string> MoveDown()
        {
            var result = _maze.Robot.MoveDown();
            var message = $"MoveDown => {result}. Robot position: ({_maze.Robot.X}, {_maze.Robot.Y})";
            _kernelBuildParameters.LogStore.Messages.Add(new LogStore.LogMessage(message, LogStore.LogType.RobotMovements));
            _maze.RecordCommand(message);
            if (_kernelBuildParameters.OnStateChangedAsync != null)
            {
                await _kernelBuildParameters.OnStateChangedAsync();
            }
            return $"{result}. Robot position: ({_maze.Robot.X}, {_maze.Robot.Y})";
        }

        [KernelFunction("MoveLeft")]
        [Description("Move the robot left one cell. Returns 'Ok' when successful, 'Err' when blocked, and appends the current robot coordinates.")]
        public async Task<string> MoveLeft()
        {
            var result = _maze.Robot.MoveLeft();
            var message = $"MoveLeft => {result}. Robot position: ({_maze.Robot.X}, {_maze.Robot.Y})";
            _kernelBuildParameters.LogStore.Messages.Add(new LogStore.LogMessage(message, LogStore.LogType.RobotMovements));
            _maze.RecordCommand(message);
            if (_kernelBuildParameters.OnStateChangedAsync != null)
            {
                await _kernelBuildParameters.OnStateChangedAsync();
            }
            return $"{result}. Robot position: ({_maze.Robot.X}, {_maze.Robot.Y})";
        }

        [KernelFunction("MoveRight")]
        [Description("Move the robot right one cell. Returns 'Ok' when successful, 'Err' when blocked, and appends the current robot coordinates.")]
        public async Task<string> MoveRight()
        {
            var result = _maze.Robot.MoveRight();
            var message = $"MoveRight => {result}. Robot position: ({_maze.Robot.X}, {_maze.Robot.Y})";
            _kernelBuildParameters.LogStore.Messages.Add(new LogStore.LogMessage(message, LogStore.LogType.RobotMovements));
            _maze.RecordCommand(message);
            if (_kernelBuildParameters.OnStateChangedAsync != null)
            {
                await _kernelBuildParameters.OnStateChangedAsync();
            }
            return $"{result}. Robot position: ({_maze.Robot.X}, {_maze.Robot.Y})";
        }

        [KernelFunction("ReportGoalReached")]
        [Description("Call this when the robot has accomplished the goal. Signals that no further movement is required.")]
        public string ReportGoalReached()
        {
            _maze.GoalAchieved = true;
            const string message = "ReportGoalReached => Goal reported as complete";
            _kernelBuildParameters.LogStore.Messages.Add(new LogStore.LogMessage(message, LogStore.LogType.RobotMovements));
            _maze.RecordCommand(message);
            return "Goal reported as complete.";
        }
    }
}
