using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using weather_backend;
using weather_backend.Services;
using Weather.API.IntegrationTests.setup;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public sealed class GeolocationControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private const string path = "/geolocation";
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public GeolocationControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetIpAddressTest()
        {
            var mockService = Substitute.For<IGeolocationService>();
            mockService.GetIpAddress().Returns("127.0.0.1");

            var client = CreateClientWithMockService(mockService);

            var response = await client.GetAsync($"{path}/ipaddress");

            response.EnsureSuccessStatusCode();
            var resultString = await response.Content.ReadAsStringAsync();
            Assert.Equal("127.0.0.1", resultString);
        }

        [Fact]
        public async Task GetLocationTest()
        {
            var mockService = Substitute.For<IGeolocationService>();
            mockService.GetLocation().Returns("Melbourne, Australia");

            var client = CreateClientWithMockService(mockService);

            var response = await client.GetAsync($"{path}/location");

            response.EnsureSuccessStatusCode();
            var resultString = await response.Content.ReadAsStringAsync();
            Assert.Equal("Melbourne, Australia", resultString);
        }

        private HttpClient CreateClientWithMockService(IGeolocationService? mockService = null)
        {
            if (mockService == null)
            {
                mockService = Substitute.For<IGeolocationService>();
            }

            return _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(mockService);
                });
            }).CreateClient();
        }
    }
}
