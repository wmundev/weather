using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using weather_backend.Models;

namespace weather_backend.Services
{
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
                using (StreamReader streamReader = new StreamReader("Assets/city.list.json"))
                {
                    string allCitiesStringified = streamReader.ReadToEnd();

                    City[] allCities = JsonConvert.DeserializeObject<City[]>(allCitiesStringified);

                    IEnumerable<City> australiaCities = allCities.Where((city) => { return city.Country == "AU"; });

                    Regex regex = new Regex("^.*?wantirna.*?$");
                    
                    foreach (City city in australiaCities)
                    {
                        MatchCollection matchCollection = regex.Matches(city.Name);
                        if (matchCollection.Count !=  0)
                        {
                            _logger.Log(LogLevel.Critical, city.Name);

                        }
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
}