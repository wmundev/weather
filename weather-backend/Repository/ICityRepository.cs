using System.Collections.Generic;
using weather_backend.Models;

namespace weather_backend.Repository
{
    public interface ICityRepository
    {
        IEnumerable<City> GetAllCitiesFromJsonFile();
    }
}
