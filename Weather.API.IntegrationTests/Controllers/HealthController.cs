using System.Threading.Tasks;
using Weather.API.IntegrationTests.setup;
using weather_backend;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public class HealthController : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public HealthController(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task HealthEndpoint()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/health");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/plain",
                response.Content.Headers.ContentType?.ToString());
            Assert.Equal("Healthy",
                await response.Content.ReadAsStringAsync());
        }
    }
}