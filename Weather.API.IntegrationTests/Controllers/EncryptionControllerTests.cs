using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using weather_backend;
using weather_backend.RequestEntities;
using weather_domain.ResponseEntities;
using Weather.API.IntegrationTests.setup;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public sealed class EncryptionControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public EncryptionControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task EncryptMessage_WhenCalledWithAValidMessage_ShouldReturnAValidResponse()
        {
            var response = await _client.GetAsync("/api/encryption/encrypt?message=hello%20world");

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var deserializedResponse = JsonSerializer.Deserialize<EncryptMessageResponse>(responseContent, Constants.CamelCaseJsonOptions);

            Assert.NotNull(deserializedResponse);
            Assert.NotNull(deserializedResponse.EncryptedMessage);
            Assert.NotNull(deserializedResponse.Key);
            Assert.NotNull(deserializedResponse.Nonce);
        }

        [Fact]
        public async Task DecryptMessage_WhenCalledWithAValidMessageAndNonceAndKey_ShouldReturnAValidResponse()
        {
            var response = await _client.PostAsync("/api/encryption/decrypt",
                new StringContent(JsonSerializer.Serialize(new DecryptMessageRequest {Message = "PrMmDeik6XGZv7ZjkD/vPVrc0xAI84FTvqkx", Nonce = "4GqruFDmklDcC41wRUWmc5r6l/O0bOIm", Key = "4Yb6iA5pem0m416luWx+PhBREUYNWssPNAUSCU3ZvFE="}, Constants.CamelCaseJsonOptions), Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var deserializedResponse = JsonSerializer.Deserialize<DecryptMessageResponse>(responseContent, Constants.CamelCaseJsonOptions);

            Assert.NotNull(deserializedResponse);
            Assert.Equal("hello world", deserializedResponse.Message);
        }
    }
}
