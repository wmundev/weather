using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using StackExchange.Redis;
using weather_backend;
using weather_backend.Dto;
using Weather.API.IntegrationTests.setup;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public class NewFeatureControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public NewFeatureControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task PatternTest()
        {
            var client = CreateClientWithMockRedis();
            var response = await client.GetAsync("/feature/pattern");
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GeneratePasswordTest()
        {
            var client = CreateClientWithMockRedis();
            var response = await client.GetAsync("/feature/password");
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task RedisSaveTest()
        {
            var mockRedis = Substitute.For<IConnectionMultiplexer>();
            var mockDb = Substitute.For<IDatabase>();
            mockRedis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(mockDb);
            mockDb.StringSetAsync("foo", "somevalue").Returns(true);

            var client = CreateClientWithMockRedis(mockRedis);

            var response = await client.PostAsJsonAsync("/feature/redis", new RedisSaveTestDto {Value = "somevalue"});

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("True", content);
        }

        [Fact]
        public async Task RedisGetTest()
        {
            var mockRedis = Substitute.For<IConnectionMultiplexer>();
            var mockDb = Substitute.For<IDatabase>();
            mockRedis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(mockDb);
            mockDb.StringGetAsync("foo").Returns(new RedisValue("somevalue"));

            var client = CreateClientWithMockRedis(mockRedis);

            var response = await client.GetAsync("/feature/redis");

            response.EnsureSuccessStatusCode();
            // The response is a JSON object { result = "somevalue", time = ... }
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("somevalue", content);
        }

        // The tests above all register a mock multiplexer, which is how the controller failing to
        // construct without one - a 500 on every /feature route - went unnoticed.
        [Theory]
        [InlineData("/feature/pattern")]
        [InlineData("/feature/password")]
        public async Task RedisFreeRoutes_WhenRedisIsNotConfigured_ShouldReturnOk(string route)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync(route);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task RedisGetTest_WhenRedisIsNotConfigured_ShouldReturn503()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/feature/redis");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Constants.CamelCaseJsonOptions);
            Assert.Equal("Redis is not configured.", problem!.Title);
        }

        [Fact]
        public async Task RedisSaveTest_WhenRedisIsNotConfigured_ShouldReturn503()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("/feature/redis", new RedisSaveTestDto {Value = "somevalue"});

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }

        private HttpClient CreateClientWithMockRedis(IConnectionMultiplexer? mockRedis = null)
        {
            if (mockRedis == null)
            {
                mockRedis = Substitute.For<IConnectionMultiplexer>();
            }

            return _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(mockRedis);
                });
            }).CreateClient();
        }
    }
}
