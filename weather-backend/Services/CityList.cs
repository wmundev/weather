using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
                using (var streamReader = new StreamReader("Assets/city.list.json"))
                {
                    var allCitiesStringify = streamReader.ReadToEnd();

                    var allCities = JsonConvert.DeserializeObject<City[]>(allCitiesStringify);
                    return allCities;
                }
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
            var allCities = GetAllCitiesFromJsonFile();
            var australiaCities = allCities.Where(city => { return city.Country == "AU"; });

            var regex = new Regex("^.*?wantirna.*?$");

            var allCitiesInAustralia = australiaCities.ToList();
            foreach (var city in allCitiesInAustralia)
            {
                var matchCollection = regex.Matches(city.Name);
                if (matchCollection.Count != 0) _logger.Log(LogLevel.Critical, city.Name);
            }

            return allCitiesInAustralia;
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