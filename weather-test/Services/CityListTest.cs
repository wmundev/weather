using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using weather_backend.Repository;
using weather_backend.Services;
using weather_domain.DatabaseEntities;
using weather_domain.Entities;
using weather_repository.City;
using Xunit;

namespace weather_test.Services
{
    public class CityListTest
    {
        private readonly CityList _cityList;
        private readonly IDynamoDbClient _dynamoDbClient;
        private readonly ILogger<CityList> _mockLogger;
        private readonly IMemoryCache _mockMemoryCache;
        private readonly ICityRepository _mockCityRepository;

        public CityListTest()
        {
            _mockLogger = Substitute.For<ILogger<CityList>>();
            _dynamoDbClient = Substitute.For<IDynamoDbClient>();

            // A real cache, not a substitute: these tests are about caching behaviour, and a substitute
            // would report a hit or a miss regardless of what was actually stored.
            _mockMemoryCache = new MemoryCache(new MemoryCacheOptions {SizeLimit = 16});
            _mockCityRepository = Substitute.For<ICityRepository>();

            _cityList = new CityList(_mockLogger, _dynamoDbClient, _mockMemoryCache, _mockCityRepository);
        }


        [Fact]
        public void GetAllCitiesInAustraliaTest()
        {
            _mockCityRepository.GetAllCitiesFromJsonFile().Returns(new List<City>
            {
                new()
                {
                    Id = 2057192,
                    Name = "Yunta",
                    State = "",
                    Country = "AU",
                    Coordinate = new Coordinate {Latitude = -32.583328, Longitude = 139.550003}
                }
            });

            var result = _cityList.GetAllCitiesInAustralia();

            var resultSerialised = JsonSerializer.Serialize(result.First());
            var expectedSerialised =
                JsonSerializer.Serialize(
                    new City
                    {
                        Id = 2057192,
                        Name = "Yunta",
                        State = "",
                        Country = "AU",
                        Coordinate = new Coordinate {Latitude = -32.583328, Longitude = 139.550003}
                    });
            Assert.Equal(resultSerialised, expectedSerialised);
        }

        [Fact]
        public async Task GetCityInfo_CachesAFoundCity()
        {
            var melbourne = new DynamoDbCity
            {
                Id = "7839805",
                Name = "Melbourne",
                State = "",
                Country = "AU",
                Coordinate = new Coordinate {Latitude = -37.8136, Longitude = 144.9631}
            };
            _mockCityRepository.GetCity("Melbourne").Returns(melbourne);

            var first = await _cityList.GetCityInfo("Melbourne");
            var second = await _cityList.GetCityInfo("Melbourne");

            Assert.Equal("Melbourne", first?.Name);
            Assert.Equal("Melbourne", second?.Name);
            await _mockCityRepository.Received(1).GetCity("Melbourne");
        }

        [Fact]
        public async Task GetCityInfo_WhenTheCityIsUnknown_DoesNotStoreACacheEntry()
        {
            _mockCityRepository.GetCity("NotACity").Returns((DynamoDbCity?) null);

            var result = await _cityList.GetCityInfo("NotACity");

            Assert.Null(result);
            // Unknown names come straight from the route, so caching them would let arbitrary input
            // fill the shared cache.
            Assert.False(_mockMemoryCache.TryGetValue("city:NotACity", out _));
        }
    }
}
