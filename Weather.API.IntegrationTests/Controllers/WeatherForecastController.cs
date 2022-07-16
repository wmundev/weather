using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using weather_backend;
using weather_backend.Models;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public class WeatherForecastController : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public WeatherForecastController(WebApplicationFactory<Startup> factory)
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