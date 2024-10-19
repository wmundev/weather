using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using weather_backend.Repository;
using weather_backend.Services;
using weather_domain.Entities;
using weather_repository.City;
using Xunit;

namespace weather_test.Services
{
    public class CityListTest
    {
        private readonly CityList _cityList;
        private readonly IDynamoDbClient _dynamoDbClient;
        private readonly IMapper _mapper;

        private readonly ILogger<CityList> _mockLogger;
        private readonly IMemoryCache _mockMemoryCache;
        private readonly ICityRepository _mockCityRepository;

        public CityListTest()
        {
            _mockLogger = Substitute.For<ILogger<CityList>>();
            _dynamoDbClient = Substitute.For<IDynamoDbClient>();
            _mapper = Substitute.For<IMapper>();
            _mockMemoryCache = Substitute.For<IMemoryCache>();
            _mockCityRepository = Substitute.For<ICityRepository>();

            _cityList = new CityList(_mockLogger, _dynamoDbClient, _mapper, _mockMemoryCache, _mockCityRepository);
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
    }
}
