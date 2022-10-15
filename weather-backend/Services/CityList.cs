using System;
using System.Collections.Generic;
using System.IO;
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


        public CityList(ILogger<CityList> logger, IDynamoDbClient dynamoDbClient, IMapper mapper, IMemoryCache memoryCache)
        {
            _logger = logger;
            _dynamoDbClient = dynamoDbClient;
            _mapper = mapper;
            _memoryCache = memoryCache;
        }

        private IEnumerable<City> GetAllCitiesFromJsonFile()
        {
            try
            {
                const string path = "./Assets/city.list.json";
                using var streamReader = new StreamReader(path);
                var allCitiesStringify = streamReader.ReadToEnd();

                var allCities = System.Text.Json.JsonSerializer.Deserialize<City[]>(allCitiesStringify);
                return allCities ?? Array.Empty<City>();
            }
            catch (IOException ioException)
            {
                Console.WriteLine("The file could not be read");
                Console.WriteLine(ioException.Message);
            }

            return Array.Empty<City>();
        }

        public async Task PopulateDynamoDbDatabase()
        {
            var allCities = GetAllCitiesFromJsonFile();

            foreach (var city in allCities)
                await _dynamoDbClient.SaveRecord(_mapper.Map<DynamoDbCity>(city));
            // System.Threading.Thread.Sleep(1000);
        }

        public IEnumerable<City> GetAllCitiesInAustralia()
        {
            const string AUSTRALIA_COUNTRY_CODE = "AU";
            var allCities = GetAllCitiesFromJsonFile();
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
