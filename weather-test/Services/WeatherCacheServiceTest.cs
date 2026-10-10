using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using weather_backend;
using weather_backend.Hosting;
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
        private readonly WeatherCacheService _sut;

        public WeatherCacheServiceTest()
        {
            // Registered the way Startup does it, including a size-limited IMemoryCache: with a SizeLimit set,
            // the memory cache throws on any entry that does not declare a size.
            var services = new ServiceCollection();
            services.AddMemoryCache(options => options.SizeLimit = WeatherApiServices.SharedMemoryCacheSizeLimitBytes);
            services.AddHybridCache();
            var cache = services.BuildServiceProvider().GetRequiredService<HybridCache>();

            _sut = new WeatherCacheService(cache, new RecordingLogger<WeatherCacheService>());
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
        public async Task GetOrFetchAsync_WhenCalledTwiceWithTheSameKey_ShouldFetchOnce()
        {
            var fetches = 0;
            Task<WeatherData> Fetch(CancellationToken _)
            {
                Interlocked.Increment(ref fetches);
                return Task.FromResult(CreateWeatherData());
            }

            var first = await _sut.GetOrFetchAsync("cityid:7839805:metric:none", Fetch);
            var second = await _sut.GetOrFetchAsync("cityid:7839805:metric:none", Fetch);

            Assert.Equal(1, fetches);
            Assert.Equal("Melbourne", first.name);
            Assert.Equal(15.5, second.main.temp);
        }

        [Fact]
        public async Task GetOrFetchAsync_WhenConcurrentCallersMissTheSameKey_ShouldShareOneFetch()
        {
            var fetches = 0;
            var release = new TaskCompletionSource<WeatherData>(TaskCreationOptions.RunContinuationsAsynchronously);
            Task<WeatherData> Fetch(CancellationToken _)
            {
                Interlocked.Increment(ref fetches);
                return release.Task;
            }

            var callers = new Task<WeatherData>[5];
            for (var i = 0; i < callers.Length; i++)
            {
                callers[i] = _sut.GetOrFetchAsync("cityid:7839805:metric:none", Fetch).AsTask();
            }

            release.SetResult(CreateWeatherData());
            var results = await Task.WhenAll(callers);

            Assert.Equal(1, fetches);
            Assert.All(results, result => Assert.Equal("Melbourne", result.name));
        }

        [Fact]
        public async Task GetOrFetchAsync_WithTheSharedCacheSizeLimit_ShouldKeepManyEntries()
        {
            // HybridCache sizes entries by payload bytes. Under the old entry-count limit of 1024, two or three
            // responses filled the shared cache and every further entry was evicted on arrival.
            var fetches = 0;
            Task<WeatherData> Fetch(CancellationToken _)
            {
                Interlocked.Increment(ref fetches);
                return Task.FromResult(CreateWeatherData());
            }

            for (var pass = 0; pass < 2; pass++)
            {
                for (var cityId = 0; cityId < 200; cityId++)
                {
                    await _sut.GetOrFetchAsync($"cityid:{cityId}:metric:none", Fetch);
                }
            }

            Assert.Equal(200, fetches);
        }

        [Fact]
        public async Task GetOrFetchAsync_ShouldNotReturnDataCachedUnderADifferentKey()
        {
            await _sut.GetOrFetchAsync("cityid:7839805:metric:none", _ => Task.FromResult(CreateWeatherData("Melbourne")));

            var london = await _sut.GetOrFetchAsync("cityid:2643743:metric:none", _ => Task.FromResult(CreateWeatherData("London")));

            Assert.Equal("London", london.name);
        }

        [Fact]
        public async Task GetOrFetchAsync_WhenTheFetchFails_ShouldNotCacheTheFailure()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.GetOrFetchAsync("cityid:1:metric:none", _ => throw new InvalidOperationException("upstream down")));

            var result = await _sut.GetOrFetchAsync("cityid:1:metric:none", _ => Task.FromResult(CreateWeatherData("Recovered")));

            Assert.Equal("Recovered", result.name);
        }
    }
}
