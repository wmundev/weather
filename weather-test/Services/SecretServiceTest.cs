using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using NSubstitute;
using weather_backend.Models;
using weather_backend.Services;
using weather_test.TestHelpers;
using Xunit;

namespace weather_test.Services
{
    public sealed class SecretServiceTest
    {
        private const string ValidSecretsJson = """
            {"OpenWeatherApiKey":"key-123","SMTPUsername":"sender@example.com","SMTPPassword":"hunter2"}
            """;

        private readonly IAmazonSimpleSystemsManagement _ssmClient = Substitute.For<IAmazonSimpleSystemsManagement>();

        private SecretService CreateSut()
        {
            return new SecretService(_ssmClient, new SecretMemoryCache(), new RecordingLogger<SecretService>());
        }

        private void SetParameterValue(string? value)
        {
            _ssmClient.GetParameterAsync(Arg.Any<GetParameterRequest>(), Arg.Any<CancellationToken>())
                .Returns(_ => Task.FromResult(new GetParameterResponse {Parameter = new Parameter {Value = value}}));
        }

        [Fact]
        public async Task GetSecrets_WhenTheDocumentIsValid_ShouldReturnEveryValue()
        {
            SetParameterValue(ValidSecretsJson);

            var result = await CreateSut().GetSecrets();

            Assert.Equal("key-123", result.OpenWeatherApiKey);
            Assert.Equal("sender@example.com", result.SMTPUsername);
            Assert.Equal("hunter2", result.SMTPPassword);
        }

        [Fact]
        public async Task GetSecrets_WhenCalledTwice_ShouldReturnTheCachedDocument()
        {
            SetParameterValue(ValidSecretsJson);
            var sut = CreateSut();

            var first = await sut.GetSecrets();
            var second = await sut.GetSecrets();

            Assert.Same(first, second);
            await _ssmClient.Received(1).GetParameterAsync(Arg.Any<GetParameterRequest>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetSecrets_WhenTheDocumentIsJsonNull_ShouldThrowAConfigurationError()
        {
            SetParameterValue("null");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => CreateSut().GetSecrets());

            Assert.Contains(nameof(AllSecrets), exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task FetchSecret_CachesSuccessfulLookups()
        {
            SetParameterValue(ValidSecretsJson);
            var sut = CreateSut();

            await sut.FetchSecret("weather_secrets");
            await sut.FetchSecret("weather_secrets");

            await _ssmClient.Received(1).GetParameterAsync(Arg.Any<GetParameterRequest>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FetchSecret_WhenTheParameterIsEmpty_Throws()
        {
            SetParameterValue("");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateSut().FetchSecret("weather_secrets"));

            Assert.Contains("weather_secrets", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task FetchSecret_WhenTheParameterIsEmpty_DoesNotCacheTheFailure()
        {
            SetParameterValue(null);
            var sut = CreateSut();

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.FetchSecret("weather_secrets"));
            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.FetchSecret("weather_secrets"));

            // A cached failure used to keep the empty value for 24 hours, so recovery needed a restart.
            await _ssmClient.Received(2).GetParameterAsync(Arg.Any<GetParameterRequest>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetSecrets_WhenTheDocumentIsIncomplete_ThrowsAConfigurationError()
        {
            SetParameterValue("{}");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateSut().GetSecrets());

            Assert.Contains(nameof(AllSecrets), exception.Message, StringComparison.Ordinal);
        }
    }
}
