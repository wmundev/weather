using System.Threading.Tasks;
using weather_backend;
using Weather.API.IntegrationTests.setup;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public sealed class PrimeNumberControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private const string path = "/api/prime-number";
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public PrimeNumberControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("5")]
        [InlineData("7")]
        [InlineData("83")]
        [InlineData("89")]
        [InlineData("97")]
        public async Task GetPrimeNumber_IsPrime_ReturnsTrue(string input)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"{path}?number={input}");

            response.EnsureSuccessStatusCode();
            Assert.Equal("true", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("1")]
        [InlineData("4")]
        [InlineData("6")]
        [InlineData("9")]
        [InlineData("84")]
        [InlineData("90")]
        [InlineData("100")]
        public async Task GetPrimeNumber_IsNotAPrime_ReturnsFalse(string input)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"{path}?number={input}");

            response.EnsureSuccessStatusCode();
            Assert.Equal("false", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task FastestIteratingOverListTest()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"{path}/haha");

            response.EnsureSuccessStatusCode();
        }
    }
}
