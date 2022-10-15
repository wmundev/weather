using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using weather_backend.Models;
using weather_backend.Repository;
using weather_backend.Services;
using Xunit;

namespace weather_test.Services
{
    public class CityListTest
    {
        private readonly CityList _cityList;
        private readonly Mock<IDynamoDbClient> _dynamoDbClient;
        private readonly Mock<IMapper> _mapper;

        private readonly Mock<ILogger<CityList>> _mockLogger;
        private readonly Mock<IMemoryCache> _mockMemoryCache;
        private readonly Mock<ICityRepository> _mockCityRepository;

        public CityListTest()
        {
            _mockLogger = new Mock<ILogger<CityList>>();
            _dynamoDbClient = new Mock<IDynamoDbClient>();
            _mapper = new Mock<IMapper>();
            _mockMemoryCache = new Mock<IMemoryCache>();
            _mockCityRepository = new Mock<ICityRepository>();

            _cityList = new CityList(_mockLogger.Object, _dynamoDbClient.Object, _mapper.Object, _mockMemoryCache.Object, _mockCityRepository.Object);
        }


        [Fact]
        public void GetAllCitiesInAustraliaTest()
        {
            _mockCityRepository.Setup(s => s.GetAllCitiesFromJsonFile()).Returns(new List<City>
            {
                new()
                {
                    Id = 2057192,
                    Name = "Yunta",
                    State = "",
                    Country = "AU",
                    Coordinate = new Coordinate { Latitude = -32.583328, Longitude = 139.550003 }
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
                        Coordinate = new Coordinate { Latitude = -32.583328, Longitude = 139.550003 }
                    });
            Assert.Equal(resultSerialised, expectedSerialised);
        }
    }
}
