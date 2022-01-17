using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using weather_backend.Models;

namespace weather_backend.Services;

public class CityList
{
    private readonly ILogger<CityList> _logger;

    public CityList(ILogger<CityList> logger)
    {
        _logger = logger;
    }

    public IEnumerable<City> GetAllCitiesInAustralia()
    {
        try
        {
            using (var streamReader = new StreamReader("Assets/city.list.json"))
            {
                var allCitiesStringified = streamReader.ReadToEnd();

                var allCities = JsonConvert.DeserializeObject<City[]>(allCitiesStringified);

                var australiaCities = allCities.Where(city => { return city.Country == "AU"; });

                var regex = new Regex("^.*?wantirna.*?$");

                foreach (var city in australiaCities)
                {
                    var matchCollection = regex.Matches(city.Name);
                    if (matchCollection.Count != 0) _logger.Log(LogLevel.Critical, city.Name);
                }

                return australiaCities;
            }
        }
        catch (IOException ioException)
        {
            Console.WriteLine("The file could not be read");
            Console.WriteLine(ioException.Message);
        }

        return null;
    }
}