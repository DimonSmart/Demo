using DimonSmart.MazeGenerator;

namespace Demo.Demos.MazeRunner;

public class MazeRunnerMaze : Maze<MazeRunnerCellModel>
{
    public MazeRunnerMaze(int width, int height) : base(width, height)
    {
        Robot = new Robot();
        Robot.PutIntoMaze(this);
    }
    public Robot Robot { get; private set; }
}
