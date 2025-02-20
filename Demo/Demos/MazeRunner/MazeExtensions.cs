using System.Text;

namespace Demo.Demos.MazeRunner
{
    /// <summary>
    /// Extension methods for MazeEnvironment.
    /// </summary>
    public static class MazeExtensions
    {
        /// <summary>
        /// Generates a detailed textual representation of the discovered parts of the maze.
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

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var cell = maze[x, y];
                    var description = GetCellDescription(cell, x, y, maze.Robot);

                    if (encloseSymbolsInQuotes)
                    {
                        sb.Append($"'{description}' ");
                    }
                    else
                    {
                        sb.Append($"{description} ");
                    }
                }
                sb.AppendLine();
            }
            var result = sb.ToString();
            return result;
        }

        /// <summary>
        /// Determines the detailed textual description for a maze cell.
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

            if (!cell.Discovered)
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
