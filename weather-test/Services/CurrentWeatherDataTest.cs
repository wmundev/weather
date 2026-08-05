using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using weather_backend.Dto;
using weather_backend.Models;
using weather_backend.Services;
using weather_backend.Services.Interfaces;
using weather_test.TestHelpers;
using Xunit;

namespace weather_test.Services
{
    public sealed class CurrentWeatherDataTest
    {
        private const string ApiKey = "super-secret-api-key";

        /// <summary>
        /// The example response documented on WeatherData, including the "base" field.
        /// </summary>
        private const string SampleResponse = """
            {
              "coord": {"lon": 144.9442, "lat": -37.8131},
              "weather": [{"id": 803, "main": "Clouds", "description": "broken clouds", "icon": "04n"}],
              "base": "stations",
              "main": {"temp": 290.78, "feels_like": 287.93, "temp_min": 289.82, "temp_max": 291.48, "pressure": 1011, "humidity": 77},
              "visibility": 10000,
              "wind": {"speed": 5.66, "deg": 180},
              "clouds": {"all": 75},
              "rain": {"1h": 1.25},
              "dt": 1613905075,
              "sys": {"type": 1, "id": 9548, "country": "AU", "sunrise": 1613850973, "sunset": 1613898712},
              "timezone": 39600,
              "id": 7839805,
              "name": "Melbourne",
              "cod": 200
            }
            """;

        private static (CurrentWeatherData sut, RecordingHttpMessageHandler handler, RecordingLogger<CurrentWeatherData> logger)
            CreateSut(string responseBody = SampleResponse)
        {
            var handler = new RecordingHttpMessageHandler(responseBody);
            var logger = new RecordingLogger<CurrentWeatherData>();

            var secretService = Substitute.For<ISecretService>();
            secretService.FetchSpecificSecret(nameof(AllSecrets.OpenWeatherApiKey)).Returns(ApiKey);

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();

            var sut = new CurrentWeatherData(configuration, new HttpClient(handler), logger, secretService);
            return (sut, handler, logger);
        }

        [Fact]
        public async Task FetchWeatherData_DoesNotLogTheApiKey()
        {
            var (sut, _, logger) = CreateSut();

            await sut.GetCurrentWeatherDataByCityId(new CityIdWeatherRequestDto {CityId = 7839805});

            Assert.NotEmpty(logger.Messages);
            Assert.DoesNotContain(logger.Messages, message => message.Contains(ApiKey, StringComparison.Ordinal));
            Assert.DoesNotContain(logger.Messages, message => message.Contains("appid", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetCurrentWeatherDataByCoordinates_FormatsCoordinatesWithTheInvariantCulture()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                // de-DE uses a comma as the decimal separator, which would produce "lat=52,52".
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");

                var (sut, handler, _) = CreateSut();

                await sut.GetCurrentWeatherDataByCoordinates(new CoordinatesWeatherRequestDto {Latitude = 52.52, Longitude = 13.405});

                var query = handler.LastRequest.Query;
                Assert.Contains("lat=52.52", query, StringComparison.Ordinal);
                Assert.Contains("lon=13.405", query, StringComparison.Ordinal);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public async Task GetCurrentWeatherDataByCityId_FormatsCityIdWithTheInvariantCulture()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");

                var (sut, handler, _) = CreateSut();

                await sut.GetCurrentWeatherDataByCityId(new CityIdWeatherRequestDto {CityId = 7839805});

                Assert.Contains("id=7839805", handler.LastRequest.Query, StringComparison.Ordinal);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public async Task FetchWeatherData_BindsFieldsWhoseJsonNameDiffersFromThePropertyName()
        {
            var (sut, _, _) = CreateSut();

            var result = await sut.GetCurrentWeatherDataByCityId(new CityIdWeatherRequestDto {CityId = 7839805});

            // These bound to nothing while the models carried Newtonsoft attributes but were
            // deserialized with System.Text.Json.
            Assert.Equal("stations", result.BaseInfo);
            Assert.Equal(1.25, result.rain?.OneHour);
        }

        [Fact]
        public async Task FetchWeatherData_BindsTheStandardFields()
        {
            var (sut, _, _) = CreateSut();

            var result = await sut.GetCurrentWeatherDataByCityId(new CityIdWeatherRequestDto {CityId = 7839805});

            Assert.Equal("Melbourne", result.name);
            Assert.Equal(290.78, result.main.temp);
            Assert.Equal(77, result.main.humidity);
            Assert.Equal("AU", result.sys.country);
            Assert.Equal(-37.8131, result.coord.Latitude);
        }

        [Fact]
        public async Task GetCurrentWeatherDataByZipCode_SendsZipAndCountry()
        {
            var (sut, handler, _) = CreateSut();

            await sut.GetCurrentWeatherDataByZipCode(new ZipCodeWeatherRequestDto {ZipCode = "90210", CountryCode = "us"});

            Assert.Contains("zip=90210%2cus", handler.LastRequest.Query, StringComparison.OrdinalIgnoreCase);
        }
    }
}
