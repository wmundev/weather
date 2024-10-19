using System.Text.Json;
using Amazon.DynamoDBv2.DataModel;
using weather_domain.DatabaseEntities;

namespace weather_repository.City
{
    public class CityRepository(IDynamoDBContext amazonDynamoDbClient) : ICityRepository
    {
        private readonly IDynamoDBContext _dynamoDbContext = amazonDynamoDbClient ?? throw new ArgumentNullException(nameof(amazonDynamoDbClient));

        public IEnumerable<weather_domain.Entities.City> GetAllCitiesFromJsonFile()
        {
            try
            {
                const string path = "./Assets/city.list.json";
                using var streamReader = new StreamReader(path);
                var allCitiesStringify = streamReader.ReadToEnd();

                var allCities = JsonSerializer.Deserialize<weather_domain.Entities.City[]>(allCitiesStringify);
                return allCities ?? Array.Empty<weather_domain.Entities.City>();
            }
            catch (IOException ioException)
            {
                Console.WriteLine("The file could not be read");
                Console.WriteLine(ioException.Message);
            }

            return Array.Empty<weather_domain.Entities.City>();
        }

        public async Task<DynamoDbCity?> GetCity(string name)
        {
            return await _dynamoDbContext.LoadAsync<DynamoDbCity>(name);
        }
    }
}
