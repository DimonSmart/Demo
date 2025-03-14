using System.Net.Http;
using System.Text;

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

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestContent = request.Content != null ? 
                await request.Content.ReadAsStringAsync(cancellationToken) : string.Empty;

            _logStore.Messages.Add(new LogStore.LogMessage(
                $"SK Request to {request.RequestUri}:\n{requestContent}", 
                LogStore.LogType.SemanticKernel));

            var response = await base.SendAsync(request, cancellationToken);
            return response;
        }
    }
}