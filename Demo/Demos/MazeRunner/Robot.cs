﻿namespace Demo.Demos.MazeRunner
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

        // Methods for observing cells in different directions.
        // For simplicity, assume the robot always looks "up".
        public MazeRunnerCellModel LookForward() => M[X, Y - 1];
        public MazeRunnerCellModel LookLeft() => M[X - 1, Y];
        public MazeRunnerCellModel LookRight() => M[X + 1, Y];
        public MazeRunnerCellModel LookBackward() => M[X, Y + 1];

        // Methods for moving the robot one cell
        public string MoveForward()
        {
            if (CanMoveTo(X, Y - 1))
            {
                Y = Y - 1;
                return "Ok";
            }
            return "Err";
        }

        public string MoveLeft()
        {
            if (CanMoveTo(X - 1, Y))
            {
                X = X - 1;
                return "Ok";
            }
            return "Err";
        }

        public string MoveRight()
        {
            if (CanMoveTo(X + 1, Y))
            {
                X = X + 1;
                return "Ok";
            }
            return "Err";
        }

        public string MoveBackward()
        {
            if (CanMoveTo(X, Y + 1))
            {
                Y = Y + 1;
                return "Ok";
            }
            return "Err";
        }

        // Method allowing the robot to mark the current cell (e.g., with chalk)
        public void MarkCell()
        {
            var cell = M[X, Y];
            cell.MarkVisited(); // Assumes Cell has a Marked property
        }

        // Private method to check if the robot can move to the specified cell
        private bool CanMoveTo(int newX, int newY)
        {
            // Check maze boundaries
            if (newX < 0 || newX >= M.Width || newY < 0 || newY >= M.Height)
            {
                return false;
            }
            // Additional checks can be added here (e.g., presence of a wall)
            return !M.IsWall(newX, newY);
        }

        public void LookAround()
        {
            if (Maze == null)
            {
                throw new InvalidOperationException("Robot is not in a maze");
            }

            for (var dx = -1; dx <= 1; dx++)
            {
                for (var dy = -1; dy <= 1; dy++)
                {
                    var newX = X + dx;
                    var newY = Y + dy;
                    if (newX >= 0 && newX < Maze.Width && newY >= 0 && newY < Maze.Height)
                    {
                        Maze[newX, newY].Discovered = true;
                    }
                }
            }
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
