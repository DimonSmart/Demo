using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Demo.Demos.MazeRunner
{
    public class MazeWalkerRobotPlugin
    {
        private readonly MazeRunnerMaze _maze;

        public MazeWalkerRobotPlugin(MazeRunnerMaze maze)
        {
            _maze = maze;
        }

        [KernelFunction("LookAround")]
        [Description("Scans the 3x3 area around the robot, marking neighboring cells as discovered, and returns a view of the explored part of the maze.")]
        public string LookAround(KernelArguments context)
        {
            _maze.Robot.LookAround();
            return _maze.MakeDiscoveredMazePartView();
        }

        [KernelFunction("MoveRight")]
        [Description("Moves the robot to the right (increases X by 1) and returns its updated coordinates.")]
        public string MoveRight(KernelArguments context)
        {
            _maze.Robot.MoveRight();
            return $"The robot moved to ({_maze.Robot.X}, {_maze.Robot.Y}).";
        }

        [KernelFunction("MoveLeft")]
        [Description("Moves the robot to the left (decreases X by 1) and returns its updated coordinates.")]
        public async Task<string> MoveLeftAsync(KernelArguments context)
        {
            _maze.Robot.MoveLeft();
            return $"The robot moved to ({_maze.Robot.X}, {_maze.Robot.Y}).";
        }

        [KernelFunction("MoveDown")]
        [Description("Moves the robot down (decreases Y by 1) and returns its updated coordinates.")]
        public async Task<string> MoveDownAsync(KernelArguments context)
        {
            _maze.Robot.MoveBackward();
            return $"The robot moved to ({_maze.Robot.X}, {_maze.Robot.Y}).";
        }

        [KernelFunction("MoveUp")]
        [Description("Moves the robot up (increases Y by 1) and returns its updated coordinates.")]
        public async Task<string> MoveUpAsync(KernelArguments context)
        {
            _maze.Robot.MoveForward();
            return $"The robot moved to ({_maze.Robot.X}, {_maze.Robot.Y}).";
        }
    }
}
