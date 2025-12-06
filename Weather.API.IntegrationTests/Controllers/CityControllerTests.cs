using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using weather_backend;
using weather_domain.DatabaseEntities;
using Weather.API.IntegrationTests.setup;
using Xunit;

namespace Weather.API.IntegrationTests.Controllers
{
    public class CityControllerTests(CustomWebApplicationFactory<Startup> factory) : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        [Fact]
        public async Task GetCityInformation()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/city/Melbourne");

            response.EnsureSuccessStatusCode();

            var stringResponse = await response.Content.ReadAsStringAsync();
            var weatherData = JsonSerializer.Deserialize<DynamoDbCity>(stringResponse, Constants.CamelCaseJsonOptions);

            Assert.Equal("Melbourne", weatherData?.Name);
        }

        [Fact]
        public async Task GetCityInformation_NotFoundTest()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/city/NonExistentCity");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCityInformation_CaseSensitiveTest()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/city/melbourne");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCityInformation_SpecialCharactersTest()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/city/Khallat%20al%20Mayyah");

            response.EnsureSuccessStatusCode();

            var stringResponse = await response.Content.ReadAsStringAsync();
            var weatherData = JsonSerializer.Deserialize<DynamoDbCity>(stringResponse, Constants.CamelCaseJsonOptions);

            Assert.Equal("Khallat al Mayyah", weatherData?.Name);
        }

        [Fact]
        public async Task GetCityInformation_EmptyStringTest()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/city/");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCities_Default_ReturnsCities()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/city/all");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var cities = JsonSerializer.Deserialize<DynamoDbCity[]>(content, Constants.CamelCaseJsonOptions); // Returns IEnumerable<City> which serializes to array

            Assert.NotNull(cities);
            Assert.NotEmpty(cities);
        }

        [Fact]
        public async Task GetCities_WithNum_ReturnsCount()
        {
            var client = factory.CreateClient();
            int limit = 5;

            var response = await client.GetAsync($"/city/all?num={limit}");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var cities = JsonSerializer.Deserialize<DynamoDbCity[]>(content, Constants.CamelCaseJsonOptions);

            Assert.NotNull(cities);
            Assert.True(cities.Length <= limit);
        }

        [Fact]
        public async Task GetCities_TooLarge_ReturnsBadRequest()
        {
            var client = factory.CreateClient();
            int limit = 600;

            var response = await client.GetAsync($"/city/all?num={limit}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
