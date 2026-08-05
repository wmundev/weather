using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using weather_backend.Dto;
using weather_backend.Models;
using weather_backend.Services;
using weather_backend.Services.Interfaces;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly CityList _cityList;
        private readonly IConfiguration _configuration;
        private readonly ICurrentWeatherData _currentWeatherData;
        private readonly EmailService _emailService;
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ISecretService _secretService;
        private readonly IWeatherCacheService _weatherCacheService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration,
            ICurrentWeatherData currentWeatherData, EmailService emailService, CityList cityList,
            ISecretService secretService, IWeatherCacheService weatherCacheService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _currentWeatherData = currentWeatherData ?? throw new ArgumentNullException(nameof(currentWeatherData));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _cityList = cityList ?? throw new ArgumentNullException(nameof(cityList));
            _secretService = secretService ?? throw new ArgumentNullException(nameof(secretService));
            _weatherCacheService = weatherCacheService ?? throw new ArgumentNullException(nameof(weatherCacheService));
        }

        /// <summary>
        /// Retrieves the current weather data for a specific city by its ID and sends an email with the weather details.
        /// </summary>
        /// <returns>
        /// A <see cref="WeatherData"/> object containing the current weather information for the specified city.
        /// </returns>
        /// <response code="201">Returns the current weather data.</response>
        /// <response code="404">Returns not found if the weather data cannot be retrieved.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/weather")]
        public async Task<WeatherData> GetCurrentWeatherDataById()
        {
            var weatherData = await _currentWeatherData.GetCurrentWeatherDataByCityId(Constants.DEFAULT_CITY_ID);

            var receiverEmail = await _secretService.FetchSpecificSecret(nameof(AllSecrets.SMTPUsername));
            if (receiverEmail is null)
            {
                throw new Exception("Receiver email in secret is null");
            }

            await _emailService.SendEmail($"{weatherData.name} Current Weather",
                $"Current Temperature: {weatherData.main.temp}, Humidity: {weatherData.main.humidity}",
                receiverEmail);
            return weatherData;
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
        [HttpGet]
        [Route("/weather/coordinates")]
        [ProducesResponseType(typeof(WeatherData), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [HttpGet]
        [Route("/weather/city")]
        [ProducesResponseType(typeof(WeatherData), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [HttpGet]
        [Route("/weather/city/{cityId}")]
        [ProducesResponseType(typeof(WeatherData), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [HttpGet]
        [Route("/weather/zip")]
        [ProducesResponseType(typeof(WeatherData), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch weather data for {Query}", logContext);
                return NotFound(new {message = notFoundMessage});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weather data for {Query}", logContext);
                return BadRequest(new {message = ex.Message});
            }
        }
    }
}
