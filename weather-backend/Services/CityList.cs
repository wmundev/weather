using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using weather_backend.Dto;
using weather_backend.Models;
using weather_backend.Repository;

namespace weather_backend.Services
{
    public class CityList
    {
        private readonly IDynamoDbClient _dynamoDbClient;
        private readonly ILogger<CityList> _logger;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;
        private readonly ICityRepository _cityRepository;


        public CityList(ILogger<CityList> logger, IDynamoDbClient dynamoDbClient, IMapper mapper, IMemoryCache memoryCache, ICityRepository cityRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dynamoDbClient = dynamoDbClient ?? throw new ArgumentNullException(nameof(dynamoDbClient));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _cityRepository = cityRepository ?? throw new ArgumentNullException(nameof(cityRepository));
        }


        public async Task PopulateDynamoDbDatabase()
        {
            var allCities = _cityRepository.GetAllCitiesFromJsonFile();

            foreach (var city in allCities)
                await _dynamoDbClient.SaveRecord(_mapper.Map<DynamoDbCity>(city));
            // System.Threading.Thread.Sleep(1000);
        }

        public IEnumerable<City> GetAllCitiesInAustralia()
        {
            const string AUSTRALIA_COUNTRY_CODE = "AU";
            var allCities = _cityRepository.GetAllCitiesFromJsonFile();
            var australiaCities = allCities.Where(city => city.Country == AUSTRALIA_COUNTRY_CODE);
            return australiaCities;
        }

        public IEnumerable<City> FilterCitiesByCityName(IEnumerable<City> cities, string cityName)
        {
            var regex = new Regex($"^.*?{cityName}.*?$");

            return cities.Where(city => regex.IsMatch(city.Name));
        }

        public async Task<DynamoDbCity> GetCityInfo(string name)
        {
            if (!_memoryCache.TryGetValue(name, out DynamoDbCity cachedCity))
            {
                Console.WriteLine("fetching from db");

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(120));

                var result = await _dynamoDbClient.GetCity(name);

                cachedCity = result;

                _memoryCache.Set(name, result, cacheEntryOptions);
            }

            return cachedCity;
        }
    }
}
