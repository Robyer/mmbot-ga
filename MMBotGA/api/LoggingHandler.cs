using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace MMBotGA.api
{
    public class LoggingHandler : DelegatingHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LoggingHandler));

        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string requestContent = request.Content != null ? await request.Content.ReadAsStringAsync(cancellationToken) : "";

            Log.Debug($"Request: {request}, Size: {requestContent.Length}");
            var response = await base.SendAsync(request, cancellationToken);

            string responseContent = response.Content != null ? await response.Content.ReadAsStringAsync(cancellationToken) : "";
            Log.Debug($"Response: {response.StatusCode}, Size: {responseContent.Length}");

            return response;
        }
    }
}
