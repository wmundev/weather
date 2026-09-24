using System.Net;
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

        // Valid values from the round-trip test above, so each case breaks exactly one thing.
        private const string ValidMessage = "PrMmDeik6XGZv7ZjkD/vPVrc0xAI84FTvqkx";
        private const string ValidNonce = "4GqruFDmklDcC41wRUWmc5r6l/O0bOIm";
        private const string ValidKey = "4Yb6iA5pem0m416luWx+PhBREUYNWssPNAUSCU3ZvFE=";

        [Theory]
        // Not Base64 at all.
        [InlineData("not-base64!", ValidNonce, ValidKey)]
        // Key too short - valid Base64 for 3 bytes rather than 32.
        [InlineData(ValidMessage, ValidNonce, "AAAA")]
        // Nonce too short - 3 bytes rather than 24.
        [InlineData(ValidMessage, "AAAA", ValidKey)]
        // Right shape, wrong key: the ciphertext fails authentication.
        [InlineData(ValidMessage, ValidNonce, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]
        public async Task DecryptMessage_WhenInputIsInvalid_ShouldReturn400(string message, string nonce, string key)
        {
            var response = await _client.PostAsync("/api/encryption/decrypt",
                new StringContent(JsonSerializer.Serialize(new DecryptMessageRequest {Message = message, Nonce = nonce, Key = key}, Constants.CamelCaseJsonOptions), Encoding.UTF8, "application/json")
            );

            // Each of these used to escape as an unhandled exception and answer 500.
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
