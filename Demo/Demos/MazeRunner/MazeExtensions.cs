using System.Text;

namespace Demo.Demos.MazeRunner
{
    /// <summary>
    /// Extension methods for the Maze environment.
    /// </summary>
    public static class MazeExtensions
    {
        /// <summary>
        /// Generates a detailed textual representation of the discovered parts of the maze.
        /// The output includes column headers, a separator line, and row numbers for easy navigation.
        /// Each cell is represented by a descriptive symbol: "Robot", "Wall", "Apple", "Pear", "Empty", or "?" if undiscovered.
        /// </summary>
        /// <param name="maze">The maze environment containing the maze and the robot.</param>
        /// <param name="encloseSymbolsInQuotes">
        /// If true, each cell symbol is enclosed in quotes (e.g., 'R', '#' etc.).
        /// </param>
        /// <returns>A string representing the discovered maze parts in a detailed textual format.</returns>
        public static string MakeDiscoveredMazePartView(this MazeRunnerMaze maze, bool encloseSymbolsInQuotes = false)
        {
            var sb = new StringBuilder();
            var width = maze.Width;
            var height = maze.Height;

            // Column header with each column number
            sb.Append("     ");
            for (var x = 0; x < width; x++)
            {
                var header = x.ToString().PadLeft(3);
                sb.Append(encloseSymbolsInQuotes ? $"'{header}' " : $"{header} ");
            }
            sb.AppendLine();

            // Separator line (optional)
            sb.Append("     ");
            for (var x = 0; x < width; x++)
            {
                sb.Append("----");
            }
            sb.AppendLine();

            // Rows with row numbers and cell descriptions
            for (var y = 0; y < height; y++)
            {
                sb.Append(y.ToString().PadLeft(3) + " | ");
                for (var x = 0; x < width; x++)
                {
                    var cell = maze[x, y];
                    var description = GetCellDescription(cell, x, y, maze.Robot);
                    sb.Append(encloseSymbolsInQuotes ? $"'{description}' " : $"{description} ");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        /// <summary>
        /// Determines the detailed textual description for a maze cell.
        /// The description is determined based on whether the cell is occupied by the robot,
        /// undiscovered, a wall, or contains specific items (Apple or Pear).
        /// </summary>
        /// <param name="cell">The maze cell.</param>
        /// <param name="x">The x coordinate of the cell.</param>
        /// <param name="y">The y coordinate of the cell.</param>
        /// <param name="robot">The robot in the maze environment.</param>
        /// <returns>A string representing the detailed description of the cell.</returns>
        private static string GetCellDescription(MazeRunnerCellModel cell, int x, int y, Robot robot)
        {
            if (robot != null && x == robot.X && y == robot.Y)
                return "Robot";

            if (!cell.IsDiscovered())
                return "?";

            if (cell.IsWall())
                return "Wall";

            var descriptions = new List<string>();
            if (cell.IsApple())
                descriptions.Add("Apple");
            if (cell.IsPear())
                descriptions.Add("Pear");

            return descriptions.Count > 0 ? string.Join(", ", descriptions) : "Empty";
        }
    }
}
