using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Weather.API.IntegrationTests.setup;
using weather_backend;
using weather_backend.Middleware;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public class CorrelationIdTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        // Any cheap route with no external dependency will do; it only has to reach the pipeline.
        private const string CheapRoute = "/api/stringbase64/encodebase64?stringToEncode=hello";

        private readonly CustomWebApplicationFactory<Program> _factory;

        public CorrelationIdTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        private static string? CorrelationIdOf(HttpResponseMessage response)
        {
            return response.Headers.TryGetValues(LogMiddleware.CorrelationIdHeader, out var values)
                ? values.FirstOrDefault()
                : null;
        }

        [Fact]
        public async Task Request_WhenCorrelationIdSupplied_ShouldEchoTheSameIdBack()
        {
            var client = _factory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, CheapRoute);
            request.Headers.Add(LogMiddleware.CorrelationIdHeader, "order-4821");

            var response = await client.SendAsync(request);

            Assert.Equal("order-4821", CorrelationIdOf(response));
        }

        [Fact]
        public async Task Request_WhenNoCorrelationIdSupplied_ShouldReturnAGeneratedOne()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync(CheapRoute);

            // A caller who did not send one still needs an id to quote when reporting a problem.
            Assert.False(string.IsNullOrEmpty(CorrelationIdOf(response)));
        }

        [Fact]
        public async Task Request_WhenCorrelationIdCarriesAHeaderInjection_ShouldNotEchoItBack()
        {
            var client = _factory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, CheapRoute);
            // TryAddWithoutValidation, because the client would otherwise reject this before it is sent -
            // and the server is what needs to be under test here.
            request.Headers.TryAddWithoutValidation(LogMiddleware.CorrelationIdHeader, "abc\r\nX-Injected: yes");

            var response = await client.SendAsync(request);

            var echoed = CorrelationIdOf(response);
            Assert.NotNull(echoed);
            Assert.DoesNotContain("X-Injected", echoed);
            Assert.False(response.Headers.Contains("X-Injected"));
        }
    }
}
