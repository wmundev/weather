using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using weather_backend;
using weather_backend.Dto;
using weather_backend.Models;
using weather_backend.Services.Interfaces;
using weather_domain.Entities;
using Weather.API.IntegrationTests.setup;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public class WeatherForecastControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public WeatherForecastControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        private WeatherData CreateValidWeatherData(string cityName)
        {
            return new WeatherData
            {
                name = cityName,
                coord = new Coordinate {Latitude = -37.8136, Longitude = 144.9631},
                weather = new[] {new weather_backend.Models.Weather {id = 800, main = "Clear", description = "clear sky", icon = "01n"}},
                main = new MainWeather
                {
                    temp = 15.0,
                    feels_like = 14.0,
                    temp_min = 10.0,
                    temp_max = 20.0,
                    pressure = 1013,
                    humidity = 70
                },
                visibility = 10000,
                wind = new Wind {speed = 5.0, deg = 180},
                clouds = new Cloud {all = 0},
                sys = new WeatherSystem
                {
                    type = 1,
                    id = 1234,
                    country = "AU",
                    sunrise = 1613850973,
                    sunset = 1613898712
                },
                dt = 1613905075,
                timezone = 39600,
                id = 7839805,
                cod = 200
            };
        }

        [Fact]
        public async Task GetCurrentWeatherDataById_Coordinates_ReturnsOk()
        {
            var mockWeather = Substitute.For<ICurrentWeatherData>();
            var expectedWeather = CreateValidWeatherData("Coordinates City");

            mockWeather.GetCurrentWeatherDataByCoordinates(Arg.Any<CoordinatesWeatherRequestDto>())
                .Returns(expectedWeather);

            var client = CreateClientWithMockService(mockWeather);

            var response = await client.GetAsync("/weather/coordinates?latitude=10.0&longitude=20.0");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<WeatherData>();
            Assert.Equal("Coordinates City", content?.name);
        }

        [Fact]
        public async Task GetCurrentWeatherDataById_CityName_ReturnsOk()
        {
            var mockWeather = Substitute.For<ICurrentWeatherData>();
            var expectedWeather = CreateValidWeatherData("London");

            mockWeather.GetCurrentWeatherDataByCityName(Arg.Any<CityNameWeatherRequestDto>())
                .Returns(expectedWeather);

            var client = CreateClientWithMockService(mockWeather);

            var response = await client.GetAsync("/weather/city?cityName=London");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<WeatherData>();
            Assert.Equal("London", content?.name);
        }

        [Fact]
        public async Task GetCurrentWeatherDataById_CityId_ReturnsOk()
        {
            var mockWeather = Substitute.For<ICurrentWeatherData>();
            var expectedWeather = CreateValidWeatherData("CityById");

            mockWeather.GetCurrentWeatherDataByCityId(Arg.Any<CityIdWeatherRequestDto>())
                .Returns(expectedWeather);

            var client = CreateClientWithMockService(mockWeather);

            var response = await client.GetAsync("/weather/city/12345");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<WeatherData>();
            Assert.Equal("CityById", content?.name);
        }

        [Fact]
        public async Task GetCurrentWeatherDataById_ZipCode_ReturnsOk()
        {
            var mockWeather = Substitute.For<ICurrentWeatherData>();
            var expectedWeather = CreateValidWeatherData("Zip City");

            mockWeather.GetCurrentWeatherDataByZipCode(Arg.Any<ZipCodeWeatherRequestDto>())
                .Returns(expectedWeather);

            var client = CreateClientWithMockService(mockWeather);

            var response = await client.GetAsync("/weather/zip?zipCode=90210");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<WeatherData>();
            Assert.Equal("Zip City", content?.name);
        }

        // [Fact]
        // public async Task GetCurrentWeatherDataByCityIdTest()
        // {
        //      var mockWeather = Substitute.For<ICurrentWeatherData>();
        //      var expectedWeather = CreateValidWeatherData("Melbourne");
        //
        //      mockWeather.GetCurrentWeatherDataByCityId(Arg.Any<double>()).Returns(expectedWeather);
        //      
        //      var mockSecret = Substitute.For<ISecretService>();
        //      mockSecret.FetchSpecificSecret(Arg.Any<string>()).Returns("test@example.com");
        //      
        //      // EmailService workaround for integration test
        //      
        //      var client = CreateClientWithMockService(mockWeather, mockSecret);
        //      
        //      try 
        //      {
        //         var response = await client.GetAsync("/weather");
        //         // response.EnsureSuccessStatusCode(); 
        //      }
        //      catch
        //      {
        //          // Ignore SMTP errors
        //      }
        // }

        [Fact]
        public async Task GetWeatherByCoordinates_NotFound_Returns404()
        {
            var mockWeather = Substitute.For<ICurrentWeatherData>();

            mockWeather.GetCurrentWeatherDataByCoordinates(Arg.Any<CoordinatesWeatherRequestDto>())
                .Returns(Task.FromException<WeatherData>(new HttpRequestException("Not Found")));

            var client = CreateClientWithMockService(mockWeather);

            var response = await client.GetAsync("/weather/coordinates?latitude=0&longitude=0");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private HttpClient CreateClientWithMockService(
            ICurrentWeatherData? mockWeather = null,
            ISecretService? mockSecret = null)
        {
            if (mockWeather == null) mockWeather = Substitute.For<ICurrentWeatherData>();
            if (mockSecret == null) mockSecret = Substitute.For<ISecretService>();

            return _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(mockWeather);
                    services.AddSingleton(mockSecret);
                });
            }).CreateClient();
        }
    }
}
