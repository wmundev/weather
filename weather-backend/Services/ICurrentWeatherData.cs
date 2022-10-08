using System.Threading.Tasks;
using weather_backend.Models;

namespace weather_backend.Services
{
    public interface ICurrentWeatherData
    {
        /**
         * Reference API: https://openweathermap.org/current "By city ID"
         * Return weather information in metric form
         */
        Task<WeatherData> GetCurrentWeatherDataByCityId(double cityId);
    }
}