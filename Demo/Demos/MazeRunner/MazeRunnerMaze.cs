using DimonSmart.MazeGenerator;
using System.Collections.Generic;

namespace Demo.Demos.MazeRunner;

public class MazeRunnerMaze : Maze<MazeRunnerCellModel>
{
    public MazeRunnerMaze(int width, int height) : base(width, height)
    {
        Robot = new Robot();
        Robot.PutIntoMaze(this);
    }
    public Robot Robot { get; private set; }

    public bool GoalAchieved { get; set; }

    public IList<string> CommandHistory { get; } = new List<string>();

    public void RecordCommand(string command)
    {
        CommandHistory.Add(command);
    }
}
