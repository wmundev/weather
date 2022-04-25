using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using weather_backend;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public class HealthController : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public HealthController(WebApplicationFactory<Startup> factory)
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
                response.Content.Headers.ContentType.ToString());
            Assert.Equal("Healthy",
                await response.Content.ReadAsStringAsync());
        }
    }
}