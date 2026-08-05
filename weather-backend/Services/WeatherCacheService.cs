using System;
using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using weather_backend.Dto;
using weather_backend.Models;

namespace weather_backend.Services
{
    public interface IWeatherCacheService
    {
        /// <summary>
        /// Get cached weather data if available and not expired
        /// </summary>
        WeatherData? GetCachedWeatherData(string cacheKey);

        /// <summary>
        /// Save weather data to cache with 1-hour TTL
        /// </summary>
        void CacheWeatherData(string cacheKey, WeatherData weatherData);

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

    /// <summary>
    /// Caches OpenWeatherMap responses in process memory for an hour.
    /// The cache is per instance and does not survive a restart; it exists to keep repeated identical
    /// queries off the upstream API, not to be a durable store.
    /// </summary>
    public class WeatherCacheService : IWeatherCacheService
    {
        private static readonly TimeSpan CacheTimeToLive = TimeSpan.FromHours(1);

        private readonly ILogger<WeatherCacheService> _logger;
        private readonly IMemoryCache _memoryCache;

        public WeatherCacheService(IMemoryCache memoryCache, ILogger<WeatherCacheService> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public WeatherData? GetCachedWeatherData(string cacheKey)
        {
            return _memoryCache.TryGetValue(cacheKey, out WeatherData? cachedData) ? cachedData : null;
        }

        public void CacheWeatherData(string cacheKey, WeatherData weatherData)
        {
            // Cache keys are built from caller-supplied query parameters, so entries carry a size and
            // the shared cache enforces a limit on how many can accumulate.
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheTimeToLive)
                .SetSize(1);

            _memoryCache.Set(cacheKey, weatherData, cacheEntryOptions);

            _logger.LogDebug("Cached weather data for key {CacheKey}", cacheKey);
        }

        public string GenerateCacheKey(CoordinatesWeatherRequestDto request)
        {
            // Formatted with the invariant culture so the key shape does not change with the host locale.
            var lat = Math.Round(request.Latitude, 4).ToString(CultureInfo.InvariantCulture);
            var lon = Math.Round(request.Longitude, 4).ToString(CultureInfo.InvariantCulture);
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
            var cityId = request.CityId.ToString(CultureInfo.InvariantCulture);
            var units = request.Units.ToString().ToLowerInvariant();
            var lang = string.IsNullOrEmpty(request.Language) ? "none" : request.Language.ToLowerInvariant();
            return $"cityid:{cityId}:{units}:{lang}";
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
