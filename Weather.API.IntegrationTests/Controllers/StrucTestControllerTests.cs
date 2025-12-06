using System.Threading.Tasks;
using weather_backend;
using Weather.API.IntegrationTests.setup;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public class StrucTestControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public StrucTestControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetStrucTest()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/struc");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("haha nice", content);
        }
    }
}
