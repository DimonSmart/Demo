using DimonSmart.MazeGenerator;

namespace Demo.Models
{
    public class MazeCell : ICell
    {
        private bool _wall = false;

        public bool IsWall() => _wall;

        public void MakeWall() => _wall = true;
    }
}
