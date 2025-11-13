namespace Demo.Demos.MazeRunner
{
    public class Robot
    {
        public int X { get; private set; } = 1;
        public int Y { get; private set; } = 1;

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

        public void SetPosition(int x, int y)
        {
            var maze = CurrentMaze;

            if (x < 0 || x >= maze.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x), "Robot X coordinate is outside of the maze.");
            }

            if (y < 0 || y >= maze.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y), "Robot Y coordinate is outside of the maze.");
            }

            if (maze.IsWall(x, y))
            {
                throw new InvalidOperationException("Robot cannot be placed on a wall cell.");
            }

            X = x;
            Y = y;
        }

        private bool CanMoveTo(int newX, int newY)
        {
            var maze = CurrentMaze;

            if (newX < 0 || newX >= maze.Width || newY < 0 || newY >= maze.Height)
            {
                return false;
            }
            return !maze.IsWall(newX, newY);
        }

        private MazeRunnerMaze CurrentMaze => Maze ?? throw new InvalidOperationException("Robot is not in a maze");
    }
}
