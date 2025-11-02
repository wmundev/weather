using System.Threading.Tasks;
using weather_backend.Dto;
using weather_backend.Models;

namespace weather_backend.Services.Interfaces
{
    public interface ICurrentWeatherData
    {
        /// <summary>
        /// Get current weather data by city ID
        /// Reference API: https://openweathermap.org/current "By city ID"
        /// </summary>
        /// <param name="cityId">The city ID from OpenWeatherMap</param>
        /// <returns>Weather data in metric units</returns>
        Task<WeatherData> GetCurrentWeatherDataByCityId(double cityId);

        /// <summary>
        /// Get current weather data by city ID with custom units and language
        /// </summary>
        /// <param name="request">Request containing city ID, units, and language preferences</param>
        /// <returns>Weather data</returns>
        Task<WeatherData> GetCurrentWeatherDataByCityId(CityIdWeatherRequestDto request);

        /// <summary>
        /// Get current weather data by geographic coordinates
        /// </summary>
        /// <param name="request">Request containing latitude, longitude, units, and language preferences</param>
        /// <returns>Weather data</returns>
        Task<WeatherData> GetCurrentWeatherDataByCoordinates(CoordinatesWeatherRequestDto request);

        /// <summary>
        /// Get current weather data by city name
        /// </summary>
        /// <param name="request">Request containing city name, optional state/country codes, units, and language preferences</param>
        /// <returns>Weather data</returns>
        Task<WeatherData> GetCurrentWeatherDataByCityName(CityNameWeatherRequestDto request);

        /// <summary>
        /// Get current weather data by ZIP/postal code
        /// </summary>
        /// <param name="request">Request containing ZIP code, country code, units, and language preferences</param>
        /// <returns>Weather data</returns>
        Task<WeatherData> GetCurrentWeatherDataByZipCode(ZipCodeWeatherRequestDto request);
    }
}
