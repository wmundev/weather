using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using weather_backend.Dto;
using weather_backend.Models;

namespace weather_backend.Repository
{
    public class CityRepository(IDynamoDBContext amazonDynamoDbClient) : ICityRepository
    {
        private readonly IDynamoDBContext _dynamoDbContext = amazonDynamoDbClient ?? throw new ArgumentNullException(nameof(amazonDynamoDbClient));

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

        public async Task<DynamoDbCity> GetCity(string name)
        {
            return await _dynamoDbContext.LoadAsync<DynamoDbCity>(name);
        }
    }
}
