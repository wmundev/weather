using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using weather_backend.Repository;
using weather_backend.Services;

namespace weather_test
{
    public class CityListTest
    {
        private readonly CityList _cityList;
        private readonly Mock<IDynamoDbClient> _dynamoDbClient;
        private readonly Mock<IMapper> _mapper;

        private readonly Mock<ILogger<CityList>> _mockLogger;
        private readonly Mock<IMemoryCache> _mockMemoryCache;

        public CityListTest()
        {
            _mockLogger = new Mock<ILogger<CityList>>();
            _dynamoDbClient = new Mock<IDynamoDbClient>();
            _mapper = new Mock<IMapper>();
            _mockMemoryCache = new Mock<IMemoryCache>();

            _cityList = new CityList(_mockLogger.Object, _dynamoDbClient.Object, _mapper.Object, _mockMemoryCache.Object);
        }

        //FIXME
        // [Fact]
        // public void GetAllCitiesInAustraliaTest()
        // {
        //     var result = _cityList.GetAllCitiesInAustralia();
        //
        //     var resultSerialised = JsonConvert.SerializeObject(result.First());
        //     var expectedSerialised =
        //         JsonConvert.SerializeObject(
        //             new City(2057192, "Yunta", "", "AU", new Coordinate(139.550003, -32.583328)));
        //     Assert.Equal(resultSerialised, expectedSerialised);
        // }
    }
}