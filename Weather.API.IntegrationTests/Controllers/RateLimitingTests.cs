using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Weather.API.IntegrationTests.setup;
using weather_backend;
using weather_backend.Dto;
using weather_backend.Models;
using weather_backend.Services.Interfaces;
using weather_domain.Entities;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public class RateLimitingTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public RateLimitingTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Each test gets its own host, and therefore its own limiter state, with limits low enough that
        /// the boundary is reached in a handful of requests rather than a hundred.
        /// </summary>
        private HttpClient CreateClientWithLimits(Dictionary<string, string?> limits,
            ICurrentWeatherData? mockWeather = null)
        {
            return _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(limits));

                if (mockWeather is not null)
                {
                    builder.ConfigureTestServices(services => services.AddSingleton(mockWeather));
                }
            }).CreateClient();
        }

        private static WeatherData CreateValidWeatherData(string cityName)
        {
            return new WeatherData
            {
                name = cityName,
                coord = new Coordinate { Latitude = -37.8136, Longitude = 144.9631 },
                weather = new[] { new weather_backend.Models.Weather { id = 800, main = "Clear", description = "clear sky", icon = "01n" } },
                main = new MainWeather { temp = 15.0, feels_like = 14.0, temp_min = 10.0, temp_max = 20.0, pressure = 1013, humidity = 70 },
                visibility = 10000,
                wind = new Wind { speed = 5.0, deg = 180 },
                clouds = new Cloud { all = 0 },
                sys = new WeatherSystem { type = 1, id = 1234, country = "AU", sunrise = 1613850973, sunset = 1613898712 },
                dt = 1613905075,
                timezone = 39600,
                id = 7839805,
                cod = 200
            };
        }

        [Fact]
        public async Task Endpoint_WhenWithinTheLimit_ShouldNotThrottle()
        {
            var client = CreateClientWithLimits(new Dictionary<string, string?>
            {
                ["RateLimiting:PermitLimit"] = "3",
                ["RateLimiting:WindowSeconds"] = "60"
            });

            for (var i = 0; i < 3; i++)
            {
                var response = await client.GetAsync("/hello-world");
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task Endpoint_WhenLimitExceeded_ShouldReturn429WithRetryAfterAndProblemDetails()
        {
            var client = CreateClientWithLimits(new Dictionary<string, string?>
            {
                ["RateLimiting:PermitLimit"] = "2",
                ["RateLimiting:WindowSeconds"] = "60"
            });

            await client.GetAsync("/hello-world");
            await client.GetAsync("/hello-world");

            var throttled = await client.GetAsync("/hello-world");

            Assert.Equal(HttpStatusCode.TooManyRequests, throttled.StatusCode);

            // Without Retry-After a caller has no way to back off correctly and will usually just spin.
            Assert.True(throttled.Headers.TryGetValues("Retry-After", out var retryAfter));
            Assert.NotEmpty(retryAfter!);

            var problem = await throttled.Content.ReadFromJsonAsync<ProblemDetails>(Constants.CamelCaseJsonOptions);
            Assert.Equal("Too many requests.", problem!.Title);
            Assert.Equal(429, problem.Status);
        }

        [Fact]
        public async Task HealthCheck_WhenLimitIsExhausted_ShouldStayExempt()
        {
            // The load balancer polls /health from a single address far more often than any caller hits
            // the API. If the limiter throttled it, healthy tasks would be pulled out of service under
            // exactly the load the limiter exists to survive.
            var client = CreateClientWithLimits(new Dictionary<string, string?>
            {
                ["RateLimiting:PermitLimit"] = "1",
                ["RateLimiting:WindowSeconds"] = "60"
            });

            for (var i = 0; i < 5; i++)
            {
                var response = await client.GetAsync("/health");
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task UncachedWeatherEndpoint_ShouldThrottleAheadOfTheGlobalLimit()
        {
            var mockWeather = Substitute.For<ICurrentWeatherData>();
            mockWeather.GetCurrentWeatherDataByCityId(Arg.Any<double>())
                .Returns(Task.FromResult(CreateValidWeatherData("Melbourne")));

            // Global limit stays generous: this proves the stricter per-endpoint policy is what bites,
            // not the global one.
            var client = CreateClientWithLimits(new Dictionary<string, string?>
            {
                ["RateLimiting:PermitLimit"] = "100",
                ["RateLimiting:WindowSeconds"] = "60",
                ["RateLimiting:UncachedPermitLimit"] = "1",
                ["RateLimiting:UncachedWindowSeconds"] = "60"
            }, mockWeather);

            var first = await client.GetAsync("/weather");
            var second = await client.GetAsync("/weather");

            Assert.Equal(HttpStatusCode.OK, first.StatusCode);
            Assert.Equal(HttpStatusCode.TooManyRequests, second.StatusCode);
        }

        [Fact]
        public async Task CachedWeatherEndpoint_ShouldNotInheritTheUncachedPolicy()
        {
            var mockWeather = Substitute.For<ICurrentWeatherData>();
            mockWeather.GetCurrentWeatherDataByCityName(Arg.Any<CityNameWeatherRequestDto>())
                .Returns(Task.FromResult(CreateValidWeatherData("Melbourne")));

            var client = CreateClientWithLimits(new Dictionary<string, string?>
            {
                ["RateLimiting:PermitLimit"] = "100",
                ["RateLimiting:WindowSeconds"] = "60",
                ["RateLimiting:UncachedPermitLimit"] = "1",
                ["RateLimiting:UncachedWindowSeconds"] = "60"
            }, mockWeather);

            var first = await client.GetAsync("/weather/city?cityName=Melbourne");
            var second = await client.GetAsync("/weather/city?cityName=Melbourne");

            Assert.Equal(HttpStatusCode.OK, first.StatusCode);
            Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        }
    }
}
