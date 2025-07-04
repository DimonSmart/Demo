namespace Demo.Demos.MazeRunner
{
    public class Robot
    {
        // Current coordinates of the robot in the maze
        public int X { get; private set; } = 1;
        public int Y { get; private set; } = 1;

        // Reference to the maze in which the robot moves
        public MazeRunnerMaze? Maze { get; private set; }

        public void PutIntoMaze(MazeRunnerMaze maze)
        {
            Maze = maze;
        }

        public bool CanMoveUp()
        {
            return CanMoveTo(X, Y - 1);
        }

        public bool CanMoveDown()
        {
            return CanMoveTo(X, Y + 1);
        }

        public bool CanMoveLeft()
        {
            return CanMoveTo(X - 1, Y);
        }

        public bool CanMoveRight()
        {
            return CanMoveTo(X + 1, Y);
        }

        // Methods for moving the robot one cell
        public string MoveUp()
        {
            if (CanMoveUp())
            {
                Y -= 1;
                return "Ok";
            }
            return "Err";
        }

        public string MoveLeft()
        {
            if (CanMoveLeft())
            {
                X -= 1;
                return "Ok";
            }
            return "Err";
        }

        public string MoveRight()
        {
            if (CanMoveRight())
            {
                X += 1;
                return "Ok";
            }
            return "Err";
        }

        public string MoveDown()
        {
            if (CanMoveDown())
            {
                Y += 1;
                return "Ok";
            }
            return "Err";
        }

        // Private method to check if the robot can move to the specified cell
        private bool CanMoveTo(int newX, int newY)
        {
            if (newX < 0 || newX >= M.Width || newY < 0 || newY >= M.Height)
            {
                return false;
            }
            return !M.IsWall(newX, newY);
        }

        private MazeRunnerMaze M
        {
            get
            {
                if (Maze == null)
                {
                    throw new InvalidOperationException("Robot is not in a maze");
                }
                return Maze;
            }
        }
    }
}
