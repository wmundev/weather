using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Polly.CircuitBreaker;
using Polly.Timeout;
using NSubstitute;
using weather_backend;
using weather_backend.Dto;
using Microsoft.AspNetCore.Mvc;
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
        public async Task GetCurrentWeatherDataById_WhenRetrieved_ShouldReturnOk()
        {
            var mockWeather = Substitute.For<ICurrentWeatherData>();
            var expectedWeather = CreateValidWeatherData("Melbourne");
            mockWeather.GetCurrentWeatherDataByCityId(weather_backend.Constants.DEFAULT_CITY_ID).Returns(expectedWeather);

            var client = CreateClientWithMockService(mockWeather);

            var response = await client.GetAsync("/weather");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<WeatherData>();
            Assert.Equal("Melbourne", content?.name);
            await mockWeather.Received(1).GetCurrentWeatherDataByCityId(weather_backend.Constants.DEFAULT_CITY_ID);
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

        [Fact]
        public async Task GetWeatherByCoordinates_WhenUpstreamTimesOut_Returns503()
        {
            var mockWeather = Substitute.For<ICurrentWeatherData>();

            // What the resilience pipeline throws once it has exhausted its retries against a slow
            // upstream. It is not an HttpRequestException, so without explicit handling it would fall
            // through to the catch-all and be reported as a 400 - blaming the caller for our outage.
            mockWeather.GetCurrentWeatherDataByCoordinates(Arg.Any<CoordinatesWeatherRequestDto>())
                .Returns(Task.FromException<WeatherData>(new TimeoutRejectedException()));

            var client = CreateClientWithMockService(mockWeather);

            var response = await client.GetAsync("/weather/coordinates?latitude=0&longitude=0");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }

        [Fact]
        public async Task GetWeatherByCityName_WhenCircuitBreakerIsOpen_Returns503()
        {
            var mockWeather = Substitute.For<ICurrentWeatherData>();

            mockWeather.GetCurrentWeatherDataByCityName(Arg.Any<CityNameWeatherRequestDto>())
                .Returns(Task.FromException<WeatherData>(new BrokenCircuitException()));

            var client = CreateClientWithMockService(mockWeather);

            var response = await client.GetAsync("/weather/city?cityName=Melbourne");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Constants.CamelCaseJsonOptions);
            Assert.Equal("Weather provider unavailable.", problem!.Title);
        }

        private HttpClient CreateClientWithMockService(ICurrentWeatherData? mockWeather = null)
        {
            if (mockWeather == null) mockWeather = Substitute.For<ICurrentWeatherData>();

            return _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(mockWeather);
                });
            }).CreateClient();
        }
    }
}
