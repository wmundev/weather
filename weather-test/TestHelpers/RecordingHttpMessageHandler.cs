using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace weather_test.TestHelpers
{
    /// <summary>
    /// An <see cref="HttpMessageHandler" /> that records the requests it is given and replies with a
    /// canned response. NSubstitute cannot intercept the protected SendAsync, so tests use this instead.
    /// </summary>
    internal sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseBody;
        private readonly HttpStatusCode _statusCode;

        public RecordingHttpMessageHandler(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _responseBody = responseBody;
            _statusCode = statusCode;
        }

        public List<Uri> Requests { get; } = new();

        public Uri LastRequest => Requests.Count > 0
            ? Requests[^1]
            : throw new InvalidOperationException("No request was sent.");

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri is not null)
            {
                Requests.Add(request.RequestUri);
            }

            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseBody)
            });
        }
    }
}
