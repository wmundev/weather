using System.Text.Json;
using System.Threading.Tasks;
using weather_backend;
using weather_backend.Dto;
using Weather.API.IntegrationTests.setup;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public class CityControllerTests(CustomWebApplicationFactory<Startup> factory) : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        [Fact]
        public async Task GetCurrentWeatherDataByCityIdTest()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/city/Melbourne");

            response.EnsureSuccessStatusCode();

            var stringResponse = await response.Content.ReadAsStringAsync();
            var weatherData = JsonSerializer.Deserialize<DynamoDbCity>(stringResponse, Constants.CamelCaseJsonOptions);

            Assert.Equal("Melbourne", weatherData?.Name);
        }
    }
}
