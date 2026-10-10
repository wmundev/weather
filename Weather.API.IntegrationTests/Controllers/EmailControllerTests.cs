using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Weather.API.IntegrationTests.setup;
using weather_backend;
using weather_backend.ActionFilter;
using weather_backend.Dto;
using weather_backend.Repository;
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

        private const string ApiKey = "test-email-code-key";

        [Fact]
        public async Task GetEmailCode_WhenTheApiKeyIsValid_ShouldReturnTheCode()
        {
            var mockDynamo = Substitute.For<IDynamoDbClient>();
            var expectedEntity = new EmailCodeEntity { Id = 123, Code = "TEST-CODE", Email = "test@example.com" };
            mockDynamo.LoadEmailCode().Returns(expectedEntity);
            var client = CreateClient(mockDynamo, ApiKey);

            var response = await client.SendAsync(Request(ApiKey));

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<EmailCodeEntity>();
            Assert.NotNull(content);
            Assert.Equal(expectedEntity.Id, content.Id);
            Assert.Equal(expectedEntity.Code, content.Code);
            Assert.Equal(expectedEntity.Email, content.Email);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("wrong-key")]
        [InlineData("test-email-code-key-but-longer")]
        public async Task GetEmailCode_WhenTheApiKeyIsMissingOrWrong_ShouldReturn401WithoutLoading(string? suppliedKey)
        {
            var mockDynamo = Substitute.For<IDynamoDbClient>();
            var client = CreateClient(mockDynamo, ApiKey);

            var response = await client.SendAsync(Request(suppliedKey));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            await mockDynamo.DidNotReceive().LoadEmailCode();
        }

        [Fact]
        public async Task GetEmailCode_WhenNoApiKeyIsConfigured_ShouldReturn503WithoutLoading()
        {
            var mockDynamo = Substitute.For<IDynamoDbClient>();
            var client = CreateClient(mockDynamo, configuredKey: null);

            var response = await client.SendAsync(Request("anything"));

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            await mockDynamo.DidNotReceive().LoadEmailCode();
        }

        private static HttpRequestMessage Request(string? apiKey)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/email/code");
            if (apiKey is not null)
            {
                request.Headers.Add(RequireApiKeyAttribute.HeaderName, apiKey);
            }

            return request;
        }

        private HttpClient CreateClient(IDynamoDbClient mockDynamo, string? configuredKey)
        {
            return _factory.WithWebHostBuilder(builder =>
            {
                // Added last so it wins over appsettings, user secrets and environment variables. Blank rather
                // than absent when unconfigured, so a key set on the developer's machine cannot leak in.
                builder.ConfigureAppConfiguration((_, configuration) =>
                    configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["EmailCode:ApiKey"] = configuredKey ?? "" }));
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(mockDynamo);
                });
            }).CreateClient();
        }
    }
}
