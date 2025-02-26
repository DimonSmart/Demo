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
        ///  - R = Robot
        ///  - # = Wall
        ///  - A = Apple
        ///  - P = Pear
        ///  - _ = Empty (discovered)
        ///  - ? = Undiscovered
        /// The output includes column headers, a separator line, and row indices for easy reference.
        /// </summary>
        /// <param name="maze">The maze environment containing the maze and the robot.</param>
        /// <param name="encloseSymbolsInQuotes">
        /// If true, each cell symbol is enclosed in quotes (e.g., 'R', '#', etc.).
        /// </param>
        /// <returns>A string representing the maze in a single-character textual format.</returns>
        public static string MakeMazeAsTextRepresentation(this MazeRunnerMaze maze, bool encloseSymbolsInQuotes = false)
        {
            var sb = new StringBuilder();
            var width = maze.Width;
            var height = maze.Height;

            // Column headers
            sb.Append("     ");
            for (var x = 0; x < width; x++)
            {
                // Use columns as numeric for clarity
                var header = x.ToString().PadLeft(2);
                sb.Append($"{header} ");
            }
            sb.AppendLine();

            // Separator line
            sb.Append("     ");
            for (var x = 0; x < width; x++)
            {
                sb.Append("---");
            }
            sb.AppendLine();

            // Each row with row index and cells
            for (var y = 0; y < height; y++)
            {
                // Row number
                sb.Append(y.ToString().PadLeft(3) + " | ");
                for (var x = 0; x < width; x++)
                {
                    var cell = maze[x, y];
                    var symbol = GetCellSymbol(cell, x, y, maze.Robot);

                    // Optionally enclose symbols in quotes
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
        /// Generates a simplified list representation of the maze cells.
        /// Each cell is represented as "x,y=symbol" (coordinates are 1-indexed)
        /// and entries are separated by semicolons.
        /// Optionally, undiscovered cells (with symbol '?') can be skipped.
        /// </summary>
        /// <param name="maze">The maze environment containing the maze and the robot.</param>
        /// <param name="encloseSymbolsInQuotes">
        /// If true, each cell symbol is enclosed in quotes.
        /// </param>
        /// <param name="includeUndiscoveredCells">
        /// If false, cells that are not discovered (with symbol '?') are not included.
        /// </param>
        /// <returns>A string representing the maze as a list of cells in the format "x,y=symbol;".</returns>
        public static string MakeMazeAsSimpleCellList(this MazeRunnerMaze maze, bool encloseSymbolsInQuotes = false, bool includeUndiscoveredCells = true)
        {
            var sb = new StringBuilder();
            var width = maze.Width;
            var height = maze.Height;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var cell = maze[x, y];
                    var symbol = GetCellSymbol(cell, x, y, maze.Robot);

                    // Skip undiscovered cells if not included
                    if (!includeUndiscoveredCells && symbol == "?")
                        continue;

                    // Optionally enclose symbol in quotes
                    var outputSymbol = encloseSymbolsInQuotes ? $"'{symbol}'" : symbol;

                    // Coordinates are 1-indexed for better readability
                    sb.Append($"{x + 1},{y + 1}={outputSymbol};");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns a single-character symbol for the cell.
        /// For discovered cells that are empty, returns '_' instead of '.'.
        /// </summary>
        private static string GetCellSymbol(MazeRunnerCellModel cell, int x, int y, Robot robot)
        {
            // Robot position takes precedence
            if (robot != null && x == robot.X && y == robot.Y)
            {
                return "R";
            }

            // Undiscovered cell
            if (!cell.IsDiscovered())
            {
                return "?";
            }

            // Wall
            if (cell.IsWall())
            {
                return "#";
            }

            // Apple or Pear (they won't coexist in the same cell)
            if (cell.IsApple())
            {
                return "A";
            }

            if (cell.IsPear())
            {
                return "P";
            }

            // Otherwise, an empty discovered cell represented as '_'
            return "_";
        }
    }
}
