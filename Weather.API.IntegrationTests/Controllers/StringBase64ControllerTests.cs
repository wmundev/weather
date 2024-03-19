using System.Threading.Tasks;
using weather_backend;
using Weather.API.IntegrationTests.setup;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public sealed class StringBase64ControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private const string path = "/api/stringbase64";
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public StringBase64ControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task DecodeBase64Test()
        {
            var client = _factory.CreateClient();
            var input = "SGVsbG8gd29ybGQ="; // "Hello world" in Base64

            var response = await client.GetAsync($"{path}/decodebase64?stringToDecode={input}");

            response.EnsureSuccessStatusCode();
            Assert.Equal("Hello world", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task EncodeBase64Test()
        {
            var client = _factory.CreateClient();
            var input = "Hello world";

            var response = await client.GetAsync($"{path}/encodebase64?stringToEncode={input}");

            response.EnsureSuccessStatusCode();
            Assert.Equal("SGVsbG8gd29ybGQ=", await response.Content.ReadAsStringAsync());
        }
    }
}
