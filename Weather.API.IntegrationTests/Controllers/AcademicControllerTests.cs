using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using weather_backend;
using weather_backend.Models;
using weather_backend.Services.Interfaces;
using Weather.API.IntegrationTests.setup;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public class AcademicControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public AcademicControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAcademicById_ReturnsAcademicInfo()
        {
            // Arrange
            var mockService = Substitute.For<IAcademicService>();
            var expectedAcademic = new Academic
            {
                ACNUM = 100,
                DEPTNUM = 10,
                FAMNAME = "Doe",
                GIVENAME = "John",
                INITIALS = "J.D.",
                TITLE = "Dr."
            };

            mockService.GetAcademicById(100).Returns(expectedAcademic);

            var client = CreateClientWithMockService(mockService);

            // Act
            var response = await client.GetAsync("/academic?id=100");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<Academic>();

            Assert.NotNull(content);
            Assert.Equal(expectedAcademic.ACNUM, content.ACNUM);
            Assert.Equal(expectedAcademic.FAMNAME, content.FAMNAME);
        }

        private HttpClient CreateClientWithMockService(IAcademicService? mockService = null)
        {
            if (mockService == null)
            {
                mockService = Substitute.For<IAcademicService>();
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
