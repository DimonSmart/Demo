using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Demo.Demos.MazeRunner
{
    public class MazeRunnerRobotPlugin // : KernelPlugin
    {
        private readonly MazeRunnerMaze _maze;


        public MazeRunnerRobotPlugin(MazeRunnerMaze maze)
        {
            _maze = maze;
        }

        [KernelFunction("LookAround")]
        [Description("Scans the 3x3 area centered on the robot, marking all neighboring cells as discovered. Returns a detailed textual representation of the maze that includes row and column indices. The output clearly distinguishes discovered cells (displaying their content), undiscovered cells (marked with '?'), walls, and special items (Apple, Pear), making it easy for both humans and LLMs to navigate the maze.")]
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
        public string MoveLeftAsync(KernelArguments context)
        {
            _maze.Robot.MoveLeft();
            return $"The robot moved to ({_maze.Robot.X}, {_maze.Robot.Y}).";
        }

        [KernelFunction("MoveDown")]
        [Description("Moves the robot down (decreases Y by 1) and returns its updated coordinates.")]
        public string MoveDown(KernelArguments context)
        {
            _maze.Robot.MoveBackward();
            return $"The robot moved to ({_maze.Robot.X}, {_maze.Robot.Y}).";
        }

        [KernelFunction("MoveUp")]
        [Description("Moves the robot up (increases Y by 1) and returns its updated coordinates.")]
        public string MoveUp(KernelArguments context)
        {
            _maze.Robot.MoveForward();
            return $"The robot moved to ({_maze.Robot.X}, {_maze.Robot.Y}).";
        }

    }
}
