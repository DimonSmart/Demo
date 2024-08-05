using DimonSmart.MazeGenerator;

namespace Demo.Models
{
    public class MazeCellModel : ICell
    {
        enum SpecialMarks
        {
            None,
            Start = 1,
            End = 2
        }

        private SpecialMarks _specialMark = SpecialMarks.None;
        private bool _wall = false;
        private int? _waveNumber;

        public bool IsStart() => _specialMark == SpecialMarks.Start;

        public bool IsEnd() => _specialMark == SpecialMarks.End;

        public bool IsWall() => _wall;

        public void MakeWall() => _wall = true;

        public void SetStart() => _specialMark = SpecialMarks.Start;

        public void SetEnd() => _specialMark = SpecialMarks.End;

        public void ClearSpecialMark() => _specialMark = SpecialMarks.None;

        public void SetWaveNumber(int waveNumber) => _waveNumber = waveNumber;

        public void ResetWaveNumber() => _waveNumber = null;

        public int? WaveNumber => _waveNumber;
    }
}
