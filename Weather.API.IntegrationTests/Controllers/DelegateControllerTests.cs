using System.Threading.Tasks;
using weather_backend;
using Weather.API.IntegrationTests.setup;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public class DelegateControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public DelegateControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetDelegateTest()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/delegates");

            response.EnsureSuccessStatusCode();
        }
    }
}
