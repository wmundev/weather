using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using weather_backend.Dto;
using weather_backend.Extensions;
using weather_backend.Models;
using weather_backend.Services;
using weather_backend.Services.Interfaces;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [ProducesResponseType(typeof(ProblemDetails), 429)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ICurrentWeatherData _currentWeatherData;
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IWeatherCacheService _weatherCacheService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            ICurrentWeatherData currentWeatherData, IWeatherCacheService weatherCacheService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentWeatherData = currentWeatherData ?? throw new ArgumentNullException(nameof(currentWeatherData));
            _weatherCacheService = weatherCacheService ?? throw new ArgumentNullException(nameof(weatherCacheService));
        }

        /// <summary>
        /// Retrieves the current weather data for the default city.
        /// </summary>
        /// <returns>
        /// A <see cref="WeatherData"/> object containing the current weather information for the specified city.
        /// </returns>
        /// <response code="200">Returns the current weather data.</response>
        /// <response code="429">If the caller has exceeded the stricter limit this endpoint carries.</response>
        [HttpGet]
        [ProducesResponseType(typeof(WeatherData), StatusCodes.Status200OK)]
        [Route("/weather")]
        // Alone among the weather routes this one bypasses the one-hour cache, so every call spends
        // OpenWeatherMap quota. It gets the tighter bucket rather than the general one.
        [EnableRateLimiting(RateLimitingExtensions.UncachedPolicy)]
        public async Task<ActionResult<WeatherData>> GetCurrentWeatherDataById()
        {
            var weatherData = await _currentWeatherData.GetCurrentWeatherDataByCityId(Constants.DEFAULT_CITY_ID);
            return Ok(weatherData);
        }

        /// <summary>
        /// Get current weather data by geographic coordinates
        /// </summary>
        /// <param name="latitude">Latitude of the location</param>
        /// <param name="longitude">Longitude of the location</param>
        /// <param name="units">Units of measurement: Standard (Kelvin), Metric (Celsius), or Imperial (Fahrenheit). Default is Metric.</param>
        /// <param name="lang">Language code for the output (e.g., en, es, fr, de, etc.)</param>
        /// <returns>Current weather data for the specified coordinates</returns>
        /// <response code="200">Returns the current weather data</response>
        /// <response code="400">If the request parameters are invalid</response>
        /// <response code="404">If weather data cannot be found for the coordinates</response>
        /// <response code="503">If the upstream weather provider is unavailable or the circuit breaker is open</response>
        /// <response code="502">If the upstream weather provider returned an error other than not found</response>
        [HttpGet]
        [Route("/weather/coordinates")]
        [ProducesResponseType(typeof(WeatherData), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<WeatherData>> GetWeatherByCoordinates(
            [FromQuery, Required] double latitude,
            [FromQuery, Required] double longitude,
            [FromQuery] WeatherUnit units = WeatherUnit.Metric,
            [FromQuery] string? lang = null)
        {
            var request = new CoordinatesWeatherRequestDto {Latitude = latitude, Longitude = longitude, Units = units, Language = lang};

            return await GetOrFetchAsync(
                _weatherCacheService.GenerateCacheKey(request),
                () => _currentWeatherData.GetCurrentWeatherDataByCoordinates(request),
                $"coordinates {latitude}, {longitude}",
                "Weather data not found for the specified coordinates");
        }

        /// <summary>
        /// Get current weather data by city name
        /// </summary>
        /// <param name="cityName">City name (required)</param>
        /// <param name="stateCode">State code (optional, only for US locations)</param>
        /// <param name="countryCode">Country code (optional, ISO 3166 country codes)</param>
        /// <param name="units">Units of measurement: Standard (Kelvin), Metric (Celsius), or Imperial (Fahrenheit). Default is Metric.</param>
        /// <param name="lang">Language code for the output (e.g., en, es, fr, de, etc.)</param>
        /// <returns>Current weather data for the specified city</returns>
        /// <response code="200">Returns the current weather data</response>
        /// <response code="400">If the request parameters are invalid</response>
        /// <response code="404">If the city cannot be found</response>
        /// <response code="503">If the upstream weather provider is unavailable or the circuit breaker is open</response>
        /// <response code="502">If the upstream weather provider returned an error other than not found</response>
        [HttpGet]
        [Route("/weather/city")]
        [ProducesResponseType(typeof(WeatherData), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<WeatherData>> GetWeatherByCityName(
            [FromQuery, Required] string cityName,
            [FromQuery] string? stateCode = null,
            [FromQuery] string? countryCode = null,
            [FromQuery] WeatherUnit units = WeatherUnit.Metric,
            [FromQuery] string? lang = null)
        {
            var request = new CityNameWeatherRequestDto
            {
                CityName = cityName,
                StateCode = stateCode,
                CountryCode = countryCode,
                Units = units,
                Language = lang
            };

            return await GetOrFetchAsync(
                _weatherCacheService.GenerateCacheKey(request),
                () => _currentWeatherData.GetCurrentWeatherDataByCityName(request),
                $"city {cityName}",
                $"Weather data not found for city: {cityName}");
        }

        /// <summary>
        /// Get current weather data by city ID
        /// </summary>
        /// <param name="cityId">OpenWeatherMap city ID</param>
        /// <param name="units">Units of measurement: Standard (Kelvin), Metric (Celsius), or Imperial (Fahrenheit). Default is Metric.</param>
        /// <param name="lang">Language code for the output (e.g., en, es, fr, de, etc.)</param>
        /// <returns>Current weather data for the specified city</returns>
        /// <response code="200">Returns the current weather data</response>
        /// <response code="400">If the request parameters are invalid</response>
        /// <response code="404">If the city ID cannot be found</response>
        /// <response code="503">If the upstream weather provider is unavailable or the circuit breaker is open</response>
        /// <response code="502">If the upstream weather provider returned an error other than not found</response>
        [HttpGet]
        [Route("/weather/city/{cityId}")]
        [ProducesResponseType(typeof(WeatherData), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<WeatherData>> GetWeatherByCityId(
            [FromRoute, Required] double cityId,
            [FromQuery] WeatherUnit units = WeatherUnit.Metric,
            [FromQuery] string? lang = null)
        {
            var request = new CityIdWeatherRequestDto {CityId = cityId, Units = units, Language = lang};

            return await GetOrFetchAsync(
                _weatherCacheService.GenerateCacheKey(request),
                () => _currentWeatherData.GetCurrentWeatherDataByCityId(request),
                $"city ID {cityId}",
                $"Weather data not found for city ID: {cityId}");
        }

        /// <summary>
        /// Get current weather data by ZIP/postal code
        /// </summary>
        /// <param name="zipCode">ZIP/postal code</param>
        /// <param name="countryCode">Country code (ISO 3166). Default is "us"</param>
        /// <param name="units">Units of measurement: Standard (Kelvin), Metric (Celsius), or Imperial (Fahrenheit). Default is Metric.</param>
        /// <param name="lang">Language code for the output (e.g., en, es, fr, de, etc.)</param>
        /// <returns>Current weather data for the specified ZIP code</returns>
        /// <response code="200">Returns the current weather data</response>
        /// <response code="400">If the request parameters are invalid</response>
        /// <response code="404">If the ZIP code cannot be found</response>
        /// <response code="503">If the upstream weather provider is unavailable or the circuit breaker is open</response>
        /// <response code="502">If the upstream weather provider returned an error other than not found</response>
        [HttpGet]
        [Route("/weather/zip")]
        [ProducesResponseType(typeof(WeatherData), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<WeatherData>> GetWeatherByZipCode(
            [FromQuery, Required] string zipCode,
            [FromQuery] string countryCode = "us",
            [FromQuery] WeatherUnit units = WeatherUnit.Metric,
            [FromQuery] string? lang = null)
        {
            var request = new ZipCodeWeatherRequestDto {ZipCode = zipCode, CountryCode = countryCode, Units = units, Language = lang};

            return await GetOrFetchAsync(
                _weatherCacheService.GenerateCacheKey(request),
                () => _currentWeatherData.GetCurrentWeatherDataByZipCode(request),
                $"ZIP code {zipCode}",
                $"Weather data not found for ZIP code: {zipCode}");
        }

        /// <summary>
        /// Serves a weather query from the cache, falling back to the OpenWeatherMap API and caching the result.
        /// </summary>
        /// <param name="cacheKey">Cache key for this query.</param>
        /// <param name="fetch">Fetches fresh data when the cache misses.</param>
        /// <param name="logContext">Description of the query used in log messages.</param>
        /// <param name="notFoundMessage">Message returned when the upstream API has no data for the query.</param>
        private async Task<ActionResult<WeatherData>> GetOrFetchAsync(
            string cacheKey,
            Func<Task<WeatherData>> fetch,
            string logContext,
            string notFoundMessage)
        {
            try
            {
                var cachedData = _weatherCacheService.GetCachedWeatherData(cacheKey);
                if (cachedData != null)
                {
                    _logger.LogInformation("Returning cached weather data for {Query}", logContext);
                    return Ok(cachedData);
                }

                _logger.LogInformation("Cache miss - fetching fresh weather data for {Query}", logContext);
                var weatherData = await fetch();

                _weatherCacheService.CacheWeatherData(cacheKey, weatherData);

                return Ok(weatherData);
            }
            // Ordered before the HttpRequestException catch: the resilience pipeline gives up on its own
            // terms rather than by returning a status code, so an upstream that is slow or has tripped the
            // circuit breaker arrives here as a timeout or a broken circuit. Neither is a caller error and
            // neither means "no such city", so they must not become a 400 or a 404.
            catch (Exception ex) when (ex is TimeoutRejectedException or BrokenCircuitException)
            {
                _logger.LogError(ex, "Upstream weather API unavailable for {Query}", logContext);
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status503ServiceUnavailable,
                        Title = "Weather provider unavailable.",
                        Detail = "The upstream weather provider did not respond in time. Please retry shortly."
                    });
            }
            // Only an upstream 404 means "no such place". Any other failure status - a rejected API key,
            // upstream throttling, a 5xx - or a request that never got a status at all is our outage,
            // not the caller's mistake, and must not be reported as a 404.
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation(ex, "Weather provider has no data for {Query}", logContext);
                return NotFound(new {message = notFoundMessage});
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                _logger.LogInformation(ex, "Weather provider rejected the query for {Query}", logContext);
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid weather query.",
                    Detail = "The weather provider rejected the query parameters."
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Weather provider request failed with {UpstreamStatus} for {Query}", ex.StatusCode, logContext);
                return StatusCode(StatusCodes.Status502BadGateway,
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status502BadGateway,
                        Title = "Weather provider error.",
                        Detail = "The upstream weather provider returned an error. Please retry shortly."
                    });
            }
            // Anything else - a missing API key, an undeserialisable response - is a server fault. It is
            // left to GlobalExceptionHandler, which answers 500 without echoing the exception text: those
            // messages name configuration keys and can carry the request URL.
        }
    }
}
