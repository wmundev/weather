using System.Collections.Generic;
using System.Threading.Tasks;
using weather_backend.Dto;
using weather_backend.Models;

namespace weather_backend.Repository
{
    public interface ICityRepository
    {
        IEnumerable<City> GetAllCitiesFromJsonFile();
        Task<DynamoDbCity> GetCity(string name);
    }
}
