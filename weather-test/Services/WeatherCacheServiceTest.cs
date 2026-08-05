using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using weather_backend.Dto;
using weather_backend.Models;
using weather_backend.Services;
using weather_domain.Entities;
using weather_test.TestHelpers;
using Xunit;

namespace weather_test.Services
{
    public sealed class WeatherCacheServiceTest
    {
        private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions {SizeLimit = 16});
        private readonly WeatherCacheService _sut;

        public WeatherCacheServiceTest()
        {
            _sut = new WeatherCacheService(_memoryCache, new RecordingLogger<WeatherCacheService>());
        }

        private static WeatherData CreateWeatherData(string name = "Melbourne")
        {
            return new WeatherData
            {
                name = name,
                coord = new Coordinate {Latitude = -37.8136, Longitude = 144.9631},
                weather = new[] {new weather_backend.Models.Weather {id = 800, main = "Clear", description = "clear sky", icon = "01n"}},
                main = new MainWeather {temp = 15.5, feels_like = 14.0, temp_min = 10.0, temp_max = 20.0, pressure = 1013, humidity = 70},
                wind = new Wind {speed = 5.0, deg = 180},
                clouds = new Cloud {all = 0},
                sys = new WeatherSystem {type = 1, id = 1234, country = "AU", sunrise = 1613850973, sunset = 1613898712}
            };
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("de-DE")]
        public void GenerateCacheKey_ForCoordinates_DoesNotVaryWithTheCurrentCulture(string culture)
        {
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);

                var key = _sut.GenerateCacheKey(new CoordinatesWeatherRequestDto {Latitude = 52.52, Longitude = 13.405});

                Assert.Equal("coordinates:52.52:13.405:metric:none", key);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("de-DE")]
        public void GenerateCacheKey_ForCityId_DoesNotVaryWithTheCurrentCulture(string culture)
        {
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);

                var key = _sut.GenerateCacheKey(new CityIdWeatherRequestDto {CityId = 7839805});

                Assert.Equal("cityid:7839805:metric:none", key);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void GenerateCacheKey_DistinguishesUnitsAndLanguage()
        {
            var metric = _sut.GenerateCacheKey(new CityIdWeatherRequestDto {CityId = 7839805, Units = WeatherUnit.Metric});
            var imperial = _sut.GenerateCacheKey(new CityIdWeatherRequestDto {CityId = 7839805, Units = WeatherUnit.Imperial});
            var german = _sut.GenerateCacheKey(new CityIdWeatherRequestDto {CityId = 7839805, Language = "de"});

            Assert.NotEqual(metric, imperial);
            Assert.NotEqual(metric, german);
        }

        [Fact]
        public void CacheWeatherData_ThenGetCachedWeatherData_ReturnsTheStoredData()
        {
            var key = _sut.GenerateCacheKey(new CityIdWeatherRequestDto {CityId = 7839805});

            _sut.CacheWeatherData(key, CreateWeatherData());
            var result = _sut.GetCachedWeatherData(key);

            Assert.NotNull(result);
            Assert.Equal("Melbourne", result.name);
            Assert.Equal(15.5, result.main.temp);
        }

        [Fact]
        public void GetCachedWeatherData_WhenTheKeyIsUnknown_ReturnsNull()
        {
            Assert.Null(_sut.GetCachedWeatherData("cityid:1:metric:none"));
        }

        [Fact]
        public void GetCachedWeatherData_DoesNotReturnDataCachedUnderADifferentKey()
        {
            var melbourneKey = _sut.GenerateCacheKey(new CityIdWeatherRequestDto {CityId = 7839805});
            var londonKey = _sut.GenerateCacheKey(new CityIdWeatherRequestDto {CityId = 2643743});

            _sut.CacheWeatherData(melbourneKey, CreateWeatherData());

            Assert.Null(_sut.GetCachedWeatherData(londonKey));
        }

        [Fact]
        public void CacheWeatherData_SetsAnEntrySizeSoTheSharedCacheStaysBounded()
        {
            // The shared cache is configured with a SizeLimit, which makes Set throw if an entry has no
            // size. Cache keys come from caller-supplied query parameters, so this bound matters.
            var exception = Record.Exception(() =>
                _sut.CacheWeatherData(_sut.GenerateCacheKey(new CityIdWeatherRequestDto {CityId = 1}), CreateWeatherData()));

            Assert.Null(exception);
        }
    }
}
