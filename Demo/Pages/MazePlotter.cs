using DimonSmart.MazeGenerator;

namespace Demo.Pages
{
    public class MazePlotter : IMazePlotter
    {
        private readonly MazeGeneratorDemo _page;
        private readonly TimeSpan _delay;
        private static readonly ConsoleColor[] colors = ((ConsoleColor[])Enum.GetValues(typeof(ConsoleColor))).Skip(1).ToArray();

        public MazePlotter(MazeGeneratorDemo page, TimeSpan delay)
        {
            _page = page;
            _delay = delay;
        }

        public void PlotWave(int x, int y, int waveNumber)
        {
          //  _page.StateHasChanged();
        }

        public void PlotPath(int x, int y, int waveNumber)
        {
          //  _page.StateHasChanged();
        }

        public void PlotWall(int x, int y)
        {
          //  await Task.Delay(_delay);
          //  _page.StateHasChanged();


        }

        public void PlotPassage(int x, int y)
        {
          //  _page.StateHasChanged();
        }
    }
}
