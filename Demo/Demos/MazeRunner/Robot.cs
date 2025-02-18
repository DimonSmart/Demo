using DimonSmart.MazeGenerator;

namespace Demo.Demos.MazeRunner;

public class Robot
{
    // Current coordinates of the robot in the maze
    public int X { get; private set; }
    public int Y { get; private set; }

    // Reference to the maze in which the robot moves
    public Maze<MazeRunnerCellModel> Maze { get; }

    public Robot(Maze<MazeRunnerCellModel> maze, int startX, int startY)
    {
        Maze = maze;
        X = startX;
        Y = startY;
    }

    // Methods for observing cells in different directions.
    // For simplicity, assume the robot always looks "up".
    public MazeRunnerCellModel LookForward() => Maze[X, Y - 1];  // forward - up (y-1)
    public MazeRunnerCellModel LookLeft() => Maze[X - 1, Y];  // left - (x-1)
    public MazeRunnerCellModel LookRight() => Maze[X + 1, Y];  // right - (x+1)
    public MazeRunnerCellModel LookBackward() => Maze[X, Y + 1];  // backward - down (y+1)

    // Methods for moving the robot one cell
    public void MoveForward()
    {
        if (CanMoveTo(X, Y - 1))
        {
            Y = Y - 1;
        }
    }
    public void MoveLeft()
    {
        if (CanMoveTo(X - 1, Y))
        {
            X = X - 1;
        }
    }
    public void MoveRight()
    {
        if (CanMoveTo(X + 1, Y))
        {
            X = X + 1;
        }
    }
    public void MoveBackward()
    {
        if (CanMoveTo(X, Y + 1))
        {
            Y = Y + 1;
        }
    }

    // Method allowing the robot to mark the current cell (e.g., with chalk)
    public void MarkCell()
    {
        var cell = Maze[X, Y];
        cell.MarkVisited(); // Assumes Cell has a Marked property
    }

    // Private method to check if the robot can move to the specified cell
    private bool CanMoveTo(int newX, int newY)
    {
        // Check maze boundaries
        if (newX < 0 || newX >= Maze.Width || newY < 0 || newY >= Maze.Height)
        {
            return false;
        }
        // Additional checks can be added here (e.g., presence of a wall)
        return !Maze.IsWall(newX, newY);
    }
}
