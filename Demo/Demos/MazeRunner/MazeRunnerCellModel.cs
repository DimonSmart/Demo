using DimonSmart.MazeGenerator;

namespace Demo.Demos.MazeRunner
{
    [Flags]
    enum SpecialMarks
    {
        None = 0,
        Apple = 1,
        Pear = 2
    }

    public class MazeRunnerCellModel : ICell
    {
        private SpecialMarks _specialMark = SpecialMarks.None;

        private bool _isWall = false;

        public bool IsWall() => _isWall;

        public void MakeWall() => _isWall = true;

        public bool IsApple() => (_specialMark & SpecialMarks.Apple) == SpecialMarks.Apple;
        public void SetApple() => _specialMark |= SpecialMarks.Apple;

        public bool IsPear() => (_specialMark & SpecialMarks.Pear) == SpecialMarks.Pear;
        public void SetPear() => _specialMark |= SpecialMarks.Pear;

        public void ClearSpecialMark() => _specialMark = SpecialMarks.None;
    }
}
