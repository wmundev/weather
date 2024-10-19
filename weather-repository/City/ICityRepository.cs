using weather_domain.DatabaseEntities;

namespace weather_repository.City
{
    public interface ICityRepository
    {
        IEnumerable<weather_domain.Entities.City> GetAllCitiesFromJsonFile();
        Task<DynamoDbCity?> GetCity(string name);
    }
}
