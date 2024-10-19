using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using weather_backend;
using Weather.API.IntegrationTests.setup;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public sealed class EnglishWordController : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private const string path = "/api/v1/word/capitalize-first-word";
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public EnglishWordController(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CapitalizeFirstWordTest()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"{path}?input=hello%20world");

            response.EnsureSuccessStatusCode();
            Assert.Equal("Hello World", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task CapitalizeFirstWordWithConjunctionTest()
        {
            var client = _factory.CreateClient();
            var query = new Dictionary<string, string> { { "input", "hello world and goodbye world" } };

            var response = await client.GetAsync(QueryHelpers.AddQueryString(path, query!));

            Assert.Equal("Hello World and Goodbye World", await response.Content.ReadAsStringAsync());
        }
    }
}
