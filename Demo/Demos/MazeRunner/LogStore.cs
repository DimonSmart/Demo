using System.Collections.ObjectModel;

namespace Demo.Demos.MazeRunner
{
    public class LogStore
    {
        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();
    }
}
