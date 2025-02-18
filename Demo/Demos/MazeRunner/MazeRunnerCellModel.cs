using DimonSmart.MazeGenerator;

namespace Demo.Demos.MazeRunner
{
    public class MazeRunnerCellModel : ICell
    {
        enum SpecialMarks
        {
            None,
            Apple = 1,
            Pear = 2
        }

        private SpecialMarks _specialMark = SpecialMarks.None;

        private bool _wall = false;
        private bool _visited = false;

        public bool IsWall() => _wall;

        public void MakeWall() => _wall = true;

        public bool IsApple() => _specialMark == SpecialMarks.Apple;
        public void SetApple() => _specialMark = SpecialMarks.Apple;

        public bool IsPear() => _specialMark == SpecialMarks.Pear;
        public void SetPear() => _specialMark = SpecialMarks.Pear;

        public void ClearSpecialMark() => _specialMark = SpecialMarks.None;
        internal void MarkVisited() => _visited = true;
    }
}
