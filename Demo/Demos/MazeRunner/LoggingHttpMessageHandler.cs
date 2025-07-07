namespace Demo.Demos.MazeRunner
{
    public class LoggingHttpMessageHandler : DelegatingHandler
    {
        private readonly LogStore _logStore;

        public LoggingHttpMessageHandler(LogStore logStore)
        {
            _logStore = logStore;
            InnerHandler = new HttpClientHandler();
        }

        public LoggingHttpMessageHandler(LogStore logStore, HttpMessageHandler innerHandler)
        {
            _logStore = logStore;
            InnerHandler = innerHandler;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestContent = request.Content != null ?
                await request.Content.ReadAsStringAsync(cancellationToken) : string.Empty;

            _logStore.Messages.Add(new LogStore.LogMessage(
                $"📤 Request: {requestContent}",
                LogStore.LogType.Http));

            var response = await base.SendAsync(request, cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logStore.Messages.Add(new LogStore.LogMessage(
                $"📥 Response: {responseContent}",
                LogStore.LogType.Http));

            return response;
        }
    }
}