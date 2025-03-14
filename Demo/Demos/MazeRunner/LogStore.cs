using System.Collections.ObjectModel;

namespace Demo.Demos.MazeRunner
{
    public class LogStore
    {
        public enum LogType
        {
            UserInput,
            SemanticKernel,
            RobotMovements,
            Error,
            Http
        }

        public record class LogMessage(string Message, LogType Type);

        public ObservableCollection<LogMessage> Messages { get; } = new ObservableCollection<LogMessage>();
    }
}
