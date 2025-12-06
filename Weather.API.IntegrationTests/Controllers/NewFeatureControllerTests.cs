using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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
