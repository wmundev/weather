using System.Net;
using System.Threading;
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
    public class MusicControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public MusicControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAsyncCancel_ReturnsBadRequest_WhenTaskIsCanceled()
        {
            // Arrange
            var mockDynamoDbClient = Substitute.For<IDynamoDbClient>();

            // Mock LoadMusicDto to simulate a task that gets canceled
            mockDynamoDbClient.LoadMusicDto(Arg.Any<CancellationToken>())
                .Returns(async x =>
                {
                    var token = x.Arg<CancellationToken>();
                    await Task.Delay(5000, token); // Simulate long running task that respects cancellation
                    return new MusicDto {Artist = "Dream Theater", SongTitle = "Surrounded"};
                });

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(mockDynamoDbClient);
                });
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/api/music/song-title");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            // Optional: Check the content
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("typeisfailed", content);
        }
    }
}
