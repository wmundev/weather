using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using weather_backend.Dto;
using weather_backend.Models;

namespace weather_backend.Services
{
    public interface IWeatherCacheService
    {
        /// <summary>
        /// Returns the cached weather data for <paramref name="cacheKey"/>, or runs <paramref name="fetch"/> and caches
        /// its result for an hour. Concurrent misses for the same key share a single fetch.
        /// </summary>
        /// <remarks>Failures are not cached: a fetch that throws leaves the key empty for the next caller.</remarks>
        ValueTask<WeatherData> GetOrFetchAsync(string cacheKey, Func<CancellationToken, Task<WeatherData>> fetch,
            CancellationToken cancellationToken = default);

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
    /// <remarks>
    /// Backed by <see cref="HybridCache"/> rather than a bare IMemoryCache get-then-set, which let every request
    /// that arrived during a miss make its own upstream call. HybridCache runs one fetch per key and hands the
    /// result to every waiting caller.
    /// </remarks>
    public class WeatherCacheService : IWeatherCacheService
    {
        private static readonly HybridCacheEntryOptions EntryOptions = new()
        {
            Expiration = TimeSpan.FromHours(1),
            LocalCacheExpiration = TimeSpan.FromHours(1)
        };

        private readonly HybridCache _cache;
        private readonly ILogger<WeatherCacheService> _logger;

        public WeatherCacheService(HybridCache cache, ILogger<WeatherCacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ValueTask<WeatherData> GetOrFetchAsync(string cacheKey, Func<CancellationToken, Task<WeatherData>> fetch,
            CancellationToken cancellationToken = default)
        {
            return _cache.GetOrCreateAsync(
                cacheKey,
                (fetch, cacheKey, _logger),
                static async (state, token) =>
                {
                    state._logger.LogInformation("Cache miss - fetching fresh weather data for {CacheKey}", state.cacheKey);
                    return await state.fetch(token);
                },
                EntryOptions,
                cancellationToken: cancellationToken);
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
