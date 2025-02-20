using Demo.Demos.MazeRunner;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace MazeDemo
{
    public class MazeRobotFunctions
    {
        private readonly MazeRunnerMaze _maze;

        public MazeRobotFunctions(MazeRunnerMaze maze)
        {
            _maze = maze;
        }

        /// <summary>
        /// Позволяет роботу осмотреть клетку впереди и возвращает описание этой клетки.
        /// </summary>
        /// <param name="context">Контекст Semantic Kernel, содержащий входные данные (если необходимо).</param>
        /// <returns>Описание клетки, находящейся впереди.</returns>


        /// <summary>
        /// Перемещает робота вправо.
        /// </summary>
        [KernelFunction("MoveRight"), Description("Moves the robot one cell to the right if possible.")]
        public string MoveRight(KernelArguments context)
        {
            return $"Robot move to ({_maze.Robot.X}, {_maze.Robot.Y}) status is: {_maze.Robot.MoveRight()}.";
        }

        /// <summary>
        /// Делает отметку в текущей клетке, где находится робот.
        /// </summary>
        [KernelFunction("MarkCell"), Description("Marks the current cell where the robot is located.")]
        public async Task<string> MarkCellAsync(KernelArguments context)
        {
            _maze.Robot.MarkCell();
            return "Current cell marked.";
        }

        // Можно добавить другие функции: LookLeft, MoveForward, MoveBackward и т.д.
    }
}
