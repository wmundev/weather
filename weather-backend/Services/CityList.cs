using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using weather_backend.Dto;
using weather_backend.Models;
using weather_backend.Repository;

namespace weather_backend.Services;

public class CityList
{
    private readonly DynamoDbClient _dynamoDbClient;
    private readonly ILogger<CityList> _logger;
    private readonly IMapper _mapper;


    public CityList(ILogger<CityList> logger, DynamoDbClient dynamoDbClient, IMapper mapper)
    {
        _logger = logger;
        _dynamoDbClient = dynamoDbClient;
        _mapper = mapper;
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
        return await _dynamoDbClient.GetCity(name);
    }
}