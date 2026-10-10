using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Weather.API.IntegrationTests.setup;
using weather_backend;
using weather_backend.Models;
using weather_backend.Services.Interfaces;
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

            mockService.GetAcademicByIdAsync(100, Arg.Any<CancellationToken>()).Returns(expectedAcademic);

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

        [Fact]
        public async Task GetAcademicById_WhenTheAcademicDoesNotExist_Returns404()
        {
            // Arrange
            var mockService = Substitute.For<IAcademicService>();
            mockService.GetAcademicByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((Academic?)null);

            var client = CreateClientWithMockService(mockService);

            // Act
            var response = await client.GetAsync("/academic?id=999999");

            // Assert: the service used to call First() on an empty result, which surfaced as a 500.
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAcademicById_WhenTheDatabaseIsNotConfigured_ShouldReturn503()
        {
            var client = _factory.WithWebHostBuilder(builder =>
                builder.ConfigureAppConfiguration((_, configuration) =>
                    configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Academic"] = "" })))
                .CreateClient();

            var response = await client.GetAsync("/academic?id=1");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }

        [Fact]
        public async Task GetAcademicById_WhenTheIdIsMissing_ShouldReturn400()
        {
            var mockService = Substitute.For<IAcademicService>();
            var client = CreateClientWithMockService(mockService);

            var response = await client.GetAsync("/academic");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            await mockService.DidNotReceiveWithAnyArgs().GetAcademicByIdAsync(default, default);
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
