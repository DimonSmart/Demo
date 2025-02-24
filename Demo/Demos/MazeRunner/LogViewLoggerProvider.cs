namespace Demo.Demos.MazeRunner
{
    public class LogViewLoggerProvider : ILoggerProvider
    {
        private readonly LogStore _logStore;

        public LogViewLoggerProvider(LogStore logStore) => _logStore = logStore;

        public ILogger CreateLogger(string categoryName)
        {
            return new LogViewLogger(categoryName, _logStore);
        }

        public void Dispose() { }
    }
}
