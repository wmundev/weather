using System;
using System.Net;
using System.Threading.Tasks;
using PhoneNumbers;
using weather_backend;
using weather_backend.Models.PhoneService;
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
            var deserialisedJsonResponse = System.Text.Json.JsonSerializer.Deserialize<ValidatePhoneNumberModel>(await response.Content.ReadAsStringAsync(), Constants.CamelCaseJsonOptions);
            Assert.NotNull(deserialisedJsonResponse);
            Assert.Equal(PhoneNumber.Types.CountryCodeSource.UNSPECIFIED, deserialisedJsonResponse.CountryCode);
            Assert.Equal(PhoneNumberType.MOBILE, deserialisedJsonResponse.NumberType);
            Assert.Equal(true, deserialisedJsonResponse.PossibleNumber);
        }
    }
}
