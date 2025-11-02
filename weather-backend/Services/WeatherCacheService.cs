using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using weather_backend.Dto;
using weather_backend.Models;
using weather_domain.DatabaseEntities;
using weather_repository;

namespace weather_backend.Services
{
    public interface IWeatherCacheService
    {
        /// <summary>
        /// Get cached weather data if available and not expired
        /// </summary>
        Task<WeatherData?> GetCachedWeatherData(string cacheKey);

        /// <summary>
        /// Save weather data to cache with 1-hour TTL
        /// </summary>
        Task CacheWeatherData(string cacheKey, string queryType, WeatherData weatherData, string? parameters = null);

        /// <summary>
        /// Generate cache key for coordinates query
        /// </summary>
        string GenerateCacheKey(CoordinatesWeatherRequestDto request);

        /// <summary>
        /// Generate cache key for city name query
        /// </summary>
        string GenerateCacheKey(CityNameWeatherRequestDto request);

        /// <summary>
        /// Generate cache key for city ID query
        /// </summary>
        string GenerateCacheKey(CityIdWeatherRequestDto request);

        /// <summary>
        /// Generate cache key for ZIP code query
        /// </summary>
        string GenerateCacheKey(ZipCodeWeatherRequestDto request);
    }

    public class WeatherCacheService : IWeatherCacheService
    {
        private readonly IWeatherCacheRepository _cacheRepository;
        private readonly ILogger<WeatherCacheService> _logger;
        private const int CacheTTLSeconds = 3600; // 1 hour

        public WeatherCacheService(IWeatherCacheRepository cacheRepository, ILogger<WeatherCacheService> logger)
        {
            _cacheRepository = cacheRepository ?? throw new ArgumentNullException(nameof(cacheRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<WeatherData?> GetCachedWeatherData(string cacheKey)
        {
            try
            {
                var cachedData = await _cacheRepository.GetCachedWeather(cacheKey);
                if (cachedData == null)
                {
                    return null;
                }

                var weatherData = JsonSerializer.Deserialize<WeatherData>(cachedData.WeatherDataJson);
                return weatherData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing cached weather data for key: {CacheKey}", cacheKey);
                return null;
            }
        }

        public async Task CacheWeatherData(string cacheKey, string queryType, WeatherData weatherData, string? parameters = null)
        {
            try
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var weatherCache = new WeatherCache
                {
                    CacheKey = cacheKey,
                    QueryType = queryType,
                    WeatherDataJson = JsonSerializer.Serialize(weatherData),
                    CreatedAt = now,
                    TTL = now + CacheTTLSeconds,
                    Parameters = parameters
                };

                await _cacheRepository.SaveWeatherCache(weatherCache);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching weather data for key: {CacheKey}", cacheKey);
                // Don't throw - caching failures should not break the API
            }
        }

        public string GenerateCacheKey(CoordinatesWeatherRequestDto request)
        {
            var lat = Math.Round(request.Latitude, 4);
            var lon = Math.Round(request.Longitude, 4);
            var units = request.Units.ToString().ToLowerInvariant();
            var lang = string.IsNullOrEmpty(request.Language) ? "none" : request.Language.ToLowerInvariant();
            return $"coordinates:{lat}:{lon}:{units}:{lang}";
        }

        public string GenerateCacheKey(CityNameWeatherRequestDto request)
        {
            var city = request.CityName.ToLowerInvariant().Replace(" ", "_");
            var state = string.IsNullOrEmpty(request.StateCode) ? "none" : request.StateCode.ToLowerInvariant();
            var country = string.IsNullOrEmpty(request.CountryCode) ? "none" : request.CountryCode.ToLowerInvariant();
            var units = request.Units.ToString().ToLowerInvariant();
            var lang = string.IsNullOrEmpty(request.Language) ? "none" : request.Language.ToLowerInvariant();
            return $"cityname:{city}:{state}:{country}:{units}:{lang}";
        }

        public string GenerateCacheKey(CityIdWeatherRequestDto request)
        {
            var units = request.Units.ToString().ToLowerInvariant();
            var lang = string.IsNullOrEmpty(request.Language) ? "none" : request.Language.ToLowerInvariant();
            return $"cityid:{request.CityId}:{units}:{lang}";
        }

        public string GenerateCacheKey(ZipCodeWeatherRequestDto request)
        {
            var zip = request.ZipCode.ToLowerInvariant();
            var country = request.CountryCode.ToLowerInvariant();
            var units = request.Units.ToString().ToLowerInvariant();
            var lang = string.IsNullOrEmpty(request.Language) ? "none" : request.Language.ToLowerInvariant();
            return $"zipcode:{zip}:{country}:{units}:{lang}";
        }
    }
}
