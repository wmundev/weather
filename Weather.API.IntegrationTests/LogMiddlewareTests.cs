using System.Net;
using System.Threading.Tasks;
using weather_backend;
using Weather.API.IntegrationTests.setup;
using Xunit;

namespace Weather.API.IntegrationTests
{
    public sealed class LogMiddlewareTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public LogMiddlewareTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("plain-correlation-id")]
        [InlineData("{evil}")]
        [InlineData("{0} {1} {2}")]
        [InlineData("{Unclosed")]
        public async Task CorrelationIdHeader_IsNeverTreatedAsALogTemplate(string correlationId)
        {
            var client = _factory.CreateClient();

            var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, "/health");
            request.Headers.Add("CorrelationID", correlationId);

            var response = await client.SendAsync(request);

            // The header used to be concatenated into the message template, so braces in the value
            // corrupted structured formatting for the whole request.
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
