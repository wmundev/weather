using System.Text.Json;
using System.Threading.Tasks;
using Weather.API.IntegrationTests.setup;
using weather_backend;
using weather_backend.Models;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public class WeatherForecastController : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public WeatherForecastController(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetCurrentWeatherDataByCityIdTest()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/weather");

            response.EnsureSuccessStatusCode();

            var stringResponse = await response.Content.ReadAsStringAsync();
            var weatherData = JsonSerializer.Deserialize<WeatherData>(stringResponse);

            Assert.Equal("Melbourne", weatherData?.name);
        }
    }
}