using System;
using System.Collections.Generic;
using System.IO;
using weather_backend.Models;

namespace weather_backend.Repository
{
    public class CityRepository : ICityRepository
    {
        public IEnumerable<City> GetAllCitiesFromJsonFile()
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
    }
}
