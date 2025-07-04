using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Demo.Demos.MazeRunner
{
    public class MazeRunnerRobotPlugin
    {
        private readonly MazeRunnerMaze _maze;
        private readonly KernelBuildParameters _kernelBuildParameters;

        public MazeRunnerRobotPlugin(KernelBuildParameters kernelBuildParameters)
        {
            _kernelBuildParameters = kernelBuildParameters;
            _maze = kernelBuildParameters.Maze;
        }

        [KernelFunction("MoveUp")]
        [Description("Move the robot up one cell. Returns 'Ok' if successful, 'Err' if blocked by wall or boundary.")]
        public async Task<string> MoveUp()
        {
            var result = _maze.Robot.MoveUp();
            var message = $"MoveUp => {result}. Robot position: ({_maze.Robot.X}, {_maze.Robot.Y})";
            _kernelBuildParameters.LogStore.Messages.Add(new LogStore.LogMessage(message, LogStore.LogType.RobotMovements));
            if (_kernelBuildParameters.OnStateChangedAsync != null)
            {
                await _kernelBuildParameters.OnStateChangedAsync();
            }
            return $"{result}. Robot position: ({_maze.Robot.X}, {_maze.Robot.Y})";
        }

        [KernelFunction("MoveDown")]
        [Description("Move the robot down one cell. Returns 'Ok' if successful, 'Err' if blocked by wall or boundary.")]
        public async Task<string> MoveDown()
        {
            var result = _maze.Robot.MoveDown();
            var message = $"MoveDown => {result}. Robot position: ({_maze.Robot.X}, {_maze.Robot.Y})";
            _kernelBuildParameters.LogStore.Messages.Add(new LogStore.LogMessage(message, LogStore.LogType.RobotMovements));
            if (_kernelBuildParameters.OnStateChangedAsync != null)
            {
                await _kernelBuildParameters.OnStateChangedAsync();
            }
            return $"{result}. Robot position: ({_maze.Robot.X}, {_maze.Robot.Y})";
        }

        [KernelFunction("MoveLeft")]
        [Description("Move the robot left one cell. Returns 'Ok' if successful, 'Err' if blocked by wall or boundary.")]
        public async Task<string> MoveLeft()
        {
            var result = _maze.Robot.MoveLeft();
            var message = $"MoveLeft => {result}. Robot position: ({_maze.Robot.X}, {_maze.Robot.Y})";
            _kernelBuildParameters.LogStore.Messages.Add(new LogStore.LogMessage(message, LogStore.LogType.RobotMovements));
            if (_kernelBuildParameters.OnStateChangedAsync != null)
            {
                await _kernelBuildParameters.OnStateChangedAsync();
            }
            return $"{result}. Robot position: ({_maze.Robot.X}, {_maze.Robot.Y})";
        }

        [KernelFunction("MoveRight")]
        [Description("Move the robot right one cell. Returns 'Ok' if successful, 'Err' if blocked by wall or boundary.")]
        public async Task<string> MoveRight()
        {
            var result = _maze.Robot.MoveRight();
            var message = $"MoveRight => {result}. Robot position: ({_maze.Robot.X}, {_maze.Robot.Y})";
            _kernelBuildParameters.LogStore.Messages.Add(new LogStore.LogMessage(message, LogStore.LogType.RobotMovements));
            if (_kernelBuildParameters.OnStateChangedAsync != null)
            {
                await _kernelBuildParameters.OnStateChangedAsync();
            }
            return $"{result}. Robot position: ({_maze.Robot.X}, {_maze.Robot.Y})";
        }

        [KernelFunction("LookAround")]
        [Description("Get a textual representation of the complete maze state showing robot position, walls, empty spaces, apples and pears. The entire maze is visible.")]
        public string LookAround()
        {
            var mazeRepresentation = _maze.MakeMazeAsTextRepresentation();
            var message = "Robot looked around and sees the complete maze";
            _kernelBuildParameters.LogStore.Messages.Add(new LogStore.LogMessage(message, LogStore.LogType.RobotMovements));
            return mazeRepresentation;
        }
    }
}
