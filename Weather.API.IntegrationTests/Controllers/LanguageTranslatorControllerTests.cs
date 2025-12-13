using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using weather_backend;
using weather_backend.Services.Interfaces;
using Weather.API.IntegrationTests.setup;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public class LanguageTranslatorControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public LanguageTranslatorControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GenerateLanguageFile_ReturnsOk()
        {
            var mockService = Substitute.For<ILanguageTranslatorService>();

            var client = CreateClientWithMockService(mockService);

            var response = await client.PostAsync("/api/LanguageTranslator/generate", null);

            response.EnsureSuccessStatusCode();
        }

        private HttpClient CreateClientWithMockService(ILanguageTranslatorService? mockService = null)
        {
            if (mockService == null)
            {
                mockService = Substitute.For<ILanguageTranslatorService>();
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
