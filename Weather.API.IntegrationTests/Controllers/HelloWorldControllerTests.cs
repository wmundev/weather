using System.Threading.Tasks;
using weather_backend;
using Weather.API.IntegrationTests.setup;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public class HelloWorldControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public HelloWorldControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetHelloWorldTest()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/hello-world");

            response.EnsureSuccessStatusCode();
            Assert.Equal("Hello World", await response.Content.ReadAsStringAsync());
        }
    }
}
