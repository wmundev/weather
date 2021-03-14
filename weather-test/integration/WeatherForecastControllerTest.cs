using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using weather_backend;
using weather_backend.Models;
using Xunit;
using Xunit.Abstractions;

namespace weather_test.integration
{
    public class WeatherForecastControllerTest
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        private readonly ITestOutputHelper _testOutputHelper;

        public WeatherForecastControllerTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            
            string pathToConfigFile =
                "C:\\Jetbrains Projects\\RiderProjects\\weather\\weather-backend\\appsettings.json";
            var config = new ConfigurationBuilder().AddJsonFile(pathToConfigFile).Build();
            var builder = new WebHostBuilder().UseStartup<Startup>().UseConfiguration(config);
            
            _server = new TestServer(builder);
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task GetCurrentWeatherDataByCityIdTest()
        {
            var response = await _client.GetAsync("/weather");
            
            response.EnsureSuccessStatusCode();

            string stringResponse = await response.Content.ReadAsStringAsync();
            WeatherData weatherData = JsonConvert.DeserializeObject<WeatherData>(stringResponse);
            // _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal("Melbourne", weatherData.name );
        }
    }
}