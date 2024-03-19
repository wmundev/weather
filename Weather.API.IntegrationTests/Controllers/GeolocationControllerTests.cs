using System.Threading.Tasks;
using weather_backend;
using Weather.API.IntegrationTests.setup;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public sealed class GeolocationControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private const string path = "/geolocation";
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public GeolocationControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetIpAddressTest()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"{path}/ipaddress");

            response.EnsureSuccessStatusCode();
            var resultString = await response.Content.ReadAsStringAsync();
            Assert.Equal("", resultString);
        }
    }
}
