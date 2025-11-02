using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using weather_domain.DatabaseEntities;

namespace weather_repository
{
    public interface IWeatherCacheRepository
    {
        /// <summary>
        /// Get cached weather data by cache key
        /// </summary>
        /// <param name="cacheKey">The cache key</param>
        /// <returns>WeatherCache object if found, null otherwise</returns>
        Task<WeatherCache?> GetCachedWeather(string cacheKey);

        /// <summary>
        /// Save weather data to cache with 1-hour TTL
        /// </summary>
        /// <param name="weatherCache">The weather cache object to save</param>
        Task SaveWeatherCache(WeatherCache weatherCache);
    }

    public class WeatherCacheRepository : IWeatherCacheRepository
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<WeatherCacheRepository> _logger;

        public WeatherCacheRepository(IDynamoDBContext dynamoDbContext, ILogger<WeatherCacheRepository> logger)
        {
            _dynamoDbContext = dynamoDbContext ?? throw new ArgumentNullException(nameof(dynamoDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<WeatherCache?> GetCachedWeather(string cacheKey)
        {
            try
            {
                _logger.LogInformation("Attempting to load cached weather data for key: {CacheKey}", cacheKey);
                var cachedData = await _dynamoDbContext.LoadAsync<WeatherCache>(cacheKey);

                if (cachedData != null)
                {
                    // Check if cache is still valid (within 1 hour)
                    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    if (cachedData.TTL > now)
                    {
                        _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
                        return cachedData;
                    }
                    else
                    {
                        _logger.LogInformation("Cache expired for key: {CacheKey}", cacheKey);
                        return null;
                    }
                }

                _logger.LogInformation("Cache miss for key: {CacheKey}", cacheKey);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cached weather data for key: {CacheKey}", cacheKey);
                return null;
            }
        }

        public async Task SaveWeatherCache(WeatherCache weatherCache)
        {
            try
            {
                _logger.LogInformation("Saving weather data to cache with key: {CacheKey}", weatherCache.CacheKey);
                await _dynamoDbContext.SaveAsync(weatherCache);
                _logger.LogInformation("Successfully saved weather data to cache");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving weather data to cache for key: {CacheKey}", weatherCache.CacheKey);
                // Don't throw - caching failures should not break the API
            }
        }
    }
}
