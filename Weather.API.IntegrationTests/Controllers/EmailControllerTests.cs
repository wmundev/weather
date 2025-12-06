using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using weather_backend;
using weather_backend.Dto;
using weather_backend.Repository;
using Weather.API.IntegrationTests.setup;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public class EmailControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public EmailControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetEmailCode_ReturnsCode()
        {
            // Arrange
            var mockDynamo = Substitute.For<IDynamoDbClient>();
            var expectedEntity = new EmailCodeEntity {Id = 123, Code = "TEST-CODE", Email = "test@example.com"};

            mockDynamo.LoadEmailCode().Returns(expectedEntity);

            var client = CreateClientWithMockDynamo(mockDynamo);

            // Act
            var response = await client.GetAsync("/api/email/code");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<EmailCodeEntity>();

            Assert.NotNull(content);
            Assert.Equal(expectedEntity.Id, content.Id);
            Assert.Equal(expectedEntity.Code, content.Code);
            Assert.Equal(expectedEntity.Email, content.Email);
        }

        private HttpClient CreateClientWithMockDynamo(IDynamoDbClient? mockDynamo = null)
        {
            if (mockDynamo == null)
            {
                mockDynamo = Substitute.For<IDynamoDbClient>();
            }

            return _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(mockDynamo);
                });
            }).CreateClient();
        }
    }
}
