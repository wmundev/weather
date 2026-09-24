using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using PhoneNumbers;
using weather_backend;
using weather_backend.Models.PhoneService;
using weather_backend.Services.Interfaces;
using Weather.API.IntegrationTests.setup;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public sealed class PhoneNumberControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private const string path = "/phone-number";
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public PhoneNumberControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        [Fact]
        public async Task ValidatePhoneNumberTest()
        {
            var client = _factory.CreateClient();
            // taken from https://en.wikipedia.org/wiki/Telephone_numbers_in_Australia
            const string input = "+610420 090 000";

            var response = await client.GetAsync($"{path}/phone?phone={input}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var deserialisedJsonResponse = JsonSerializer.Deserialize<ValidatePhoneNumberModel>(await response.Content.ReadAsStringAsync(), Constants.CamelCaseJsonOptions);
            Assert.NotNull(deserialisedJsonResponse);
            Assert.Equal(PhoneNumber.Types.CountryCodeSource.UNSPECIFIED, deserialisedJsonResponse.CountryCode);
            Assert.Equal(PhoneNumberType.MOBILE, deserialisedJsonResponse.NumberType);
            Assert.True(deserialisedJsonResponse.PossibleNumber);
        }

        [Fact]
        public async Task ValidatePhoneNumber_InvalidInput_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            const string input = "implementing_this_feature_was_a_mistake";

            var response = await client.GetAsync($"{path}/phone?phone={input}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ValidatePhoneNumber_WhenServiceFails_ShouldReturn500WithoutExceptionText()
        {
            var phoneService = Substitute.For<IPhoneService>();
            phoneService.ValidatePhoneNumber(Arg.Any<string>())
                .Returns(_ => throw new InvalidOperationException("internal detail"));

            // Production, because Development deliberately shows exception detail to the developer.
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment(Environments.Production);
                // Only appsettings.Development.json names a region; nothing here calls AWS.
                builder.UseSetting("AWS:Region", "us-east-1");
                builder.ConfigureTestServices(services => services.AddSingleton(phoneService));
            }).CreateClient();

            var response = await client.GetAsync($"{path}/phone?phone=0420090000");

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.DoesNotContain("internal detail", await response.Content.ReadAsStringAsync());
        }
    }
}
