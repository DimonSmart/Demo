using DimonSmart.MazeGenerator;

namespace Demo.Demos.MazeRunner;

public class MazeRunnerMaze(int width, int height, Robot robot) : Maze<MazeRunnerCellModel>(width, height)
{
    public Robot Robot { get; } = robot;
}
