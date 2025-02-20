namespace Demo.Demos.MazeRunner
{
    public class LogViewLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly LogStore _logStore;

        public LogViewLogger(string categoryName, LogStore logStore)
        {
            _categoryName = categoryName;
            _logStore = logStore;
        }

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                                Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            var formattedMessage = $"[{DateTime.Now:HH:mm:ss}] {_categoryName} {logLevel}: {message}";
            _logStore.Messages.Add(formattedMessage);
        }
    }
}
