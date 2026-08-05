using System.Text.Json;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using weather_domain.DatabaseEntities;

namespace weather_repository.City
{
    public class CityRepository : ICityRepository
    {
        private const string CityListPath = "Assets/city.list.json";

        private readonly Lazy<IReadOnlyList<weather_domain.Entities.City>> _allCities;
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<CityRepository> _logger;

        public CityRepository(IDynamoDBContext amazonDynamoDbClient, ILogger<CityRepository> logger)
        {
            _dynamoDbContext = amazonDynamoDbClient ?? throw new ArgumentNullException(nameof(amazonDynamoDbClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // The city dataset is a ~40 MB file. This repository is a singleton, so it is parsed once
            // for the lifetime of the process instead of once per request.
            _allCities = new Lazy<IReadOnlyList<weather_domain.Entities.City>>(
                LoadAllCities, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public IEnumerable<weather_domain.Entities.City> GetAllCitiesFromJsonFile()
        {
            return _allCities.Value;
        }

        public async Task<DynamoDbCity?> GetCity(string name)
        {
            return await _dynamoDbContext.LoadAsync<DynamoDbCity>(name);
        }

        private IReadOnlyList<weather_domain.Entities.City> LoadAllCities()
        {
            var fullPath = Path.GetFullPath(CityListPath);

            try
            {
                // Streamed rather than read into a string first: materialising the whole file put a
                // ~40 MB string on the large object heap on every call.
                using var stream = File.OpenRead(CityListPath);
                var allCities = JsonSerializer.Deserialize<weather_domain.Entities.City[]>(stream);

                if (allCities is null)
                {
                    _logger.LogError("City dataset at {Path} deserialized to null", fullPath);
                    return Array.Empty<weather_domain.Entities.City>();
                }

                _logger.LogInformation("Loaded {CityCount} cities from {Path}", allCities.Length, fullPath);
                return allCities;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                _logger.LogError(exception, "Could not read the city dataset at {Path}. City listing endpoints will return no results", fullPath);
            }
            catch (JsonException exception)
            {
                _logger.LogError(exception, "The city dataset at {Path} is not valid JSON. City listing endpoints will return no results", fullPath);
            }

            return Array.Empty<weather_domain.Entities.City>();
        }
    }
}
