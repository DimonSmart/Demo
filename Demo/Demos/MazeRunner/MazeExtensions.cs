using System.Text;

namespace Demo.Demos.MazeRunner
{
    /// <summary>
    /// Extension methods for the Maze environment.
    /// </summary>
    public static class MazeExtensions
    {
        /// <summary>
        /// Generates a textual representation of the maze using single-character symbols for each cell:
        ///  - R = Robot (coordinates are zero-based, e.g. 0,0 is top-left)
        ///  - # = Wall
        ///  - A = Apple
        ///  - P = Pear
        ///  - _ = Empty space
        /// The output includes column headers, a separator line, and row indices for easy reference.
        /// Coordinates are zero-based to match internal representation.
        /// </summary>
        /// <param name="maze">The maze environment containing the maze and the robot.</param>
        /// <param name="encloseSymbolsInQuotes">
        /// If true, each cell symbol is enclosed in quotes (e.g., 'R', '#', etc.).
        /// </param>
        /// <returns>A string representing the maze in a single-character textual format with zero-based coordinates.</returns>
        public static string MakeMazeAsTextRepresentation(this MazeRunnerMaze maze, bool encloseSymbolsInQuotes = false)
        {
            var sb = new StringBuilder();
            var width = maze.Width;
            var height = maze.Height;

            // Add coordinate system explanation
            sb.AppendLine("# Maze representation with zero-based coordinates (0,0 at top-left)");
            sb.AppendLine("# Robot position (x, y): " + (maze.Robot != null ? $"x={maze.Robot.X}, y={maze.Robot.Y}" : "not placed"));
            sb.AppendLine();

            // Column headers
            sb.Append("     ");
            for (var x = 0; x < width; x++)
            {
                var header = x.ToString().PadLeft(2);
                sb.Append($"{header} ");
            }
            sb.AppendLine();

            // Separator line with coordinate axis marker
            sb.Append("   y\\x");
            for (var x = 0; x < width; x++)
            {
                sb.Append("---");
            }
            sb.AppendLine();

            // Each row with row index and cells
            for (var y = 0; y < height; y++)
            {
                sb.Append(y.ToString().PadLeft(3) + " | ");
                for (var x = 0; x < width; x++)
                {
                    var cell = maze[x, y];
                    var symbol = GetCellSymbol(cell, x, y, maze.Robot);

                    if (encloseSymbolsInQuotes)
                    {
                        sb.Append($"'{symbol}' ");
                    }
                    else
                    {
                        sb.Append($"{symbol}  "); // extra space for better spacing
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns a single-character symbol for the cell.
        /// </summary>
        private static string GetCellSymbol(MazeRunnerCellModel cell, int x, int y, Robot? robot)
        {
            // Robot position takes precedence
            if (robot != null && x == robot.X && y == robot.Y)
            {
                return "R";
            }

            if (cell.IsWall())
            {
                return "#";
            }

            if (cell.IsApple())
            {
                return "A";
            }

            if (cell.IsPear())
            {
                return "P";
            }

            // Empty cell represented as '_'
            return "_";
        }
    }
}
