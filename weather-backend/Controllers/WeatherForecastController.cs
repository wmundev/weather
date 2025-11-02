using System;
using System.ComponentModel.DataAnnotations;
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
            //melbourne cityid: 7839805
            double cityId = 7839805;
            var weatherData = await _currentWeatherData.GetCurrentWeatherDataByCityId(cityId);

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
            try
            {
                var request = new CoordinatesWeatherRequestDto {Latitude = latitude, Longitude = longitude, Units = units, Language = lang};

                // Generate cache key
                var cacheKey = _weatherCacheService.GenerateCacheKey(request);

                // Try to get from cache
                var cachedData = await _weatherCacheService.GetCachedWeatherData(cacheKey);
                if (cachedData != null)
                {
                    _logger.LogInformation("Returning cached weather data for coordinates: {Latitude}, {Longitude}", latitude, longitude);
                    return Ok(cachedData);
                }

                // Cache miss - fetch from API
                _logger.LogInformation("Cache miss - fetching fresh weather data for coordinates: {Latitude}, {Longitude}", latitude, longitude);
                var weatherData = await _currentWeatherData.GetCurrentWeatherDataByCoordinates(request);

                // Cache the result
                await _weatherCacheService.CacheWeatherData(
                    cacheKey,
                    "coordinates",
                    weatherData,
                    $"lat:{latitude},lon:{longitude},units:{units},lang:{lang ?? "none"}");

                return Ok(weatherData);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch weather data for coordinates: {Latitude}, {Longitude}", latitude, longitude);
                return NotFound(new {message = "Weather data not found for the specified coordinates"});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weather data for coordinates: {Latitude}, {Longitude}", latitude, longitude);
                return BadRequest(new {message = ex.Message});
            }
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
            try
            {
                var request = new CityNameWeatherRequestDto
                {
                    CityName = cityName,
                    StateCode = stateCode,
                    CountryCode = countryCode,
                    Units = units,
                    Language = lang
                };

                // Generate cache key
                var cacheKey = _weatherCacheService.GenerateCacheKey(request);

                // Try to get from cache
                var cachedData = await _weatherCacheService.GetCachedWeatherData(cacheKey);
                if (cachedData != null)
                {
                    _logger.LogInformation("Returning cached weather data for city: {CityName}", cityName);
                    return Ok(cachedData);
                }

                // Cache miss - fetch from API
                _logger.LogInformation("Cache miss - fetching fresh weather data for city: {CityName}", cityName);
                var weatherData = await _currentWeatherData.GetCurrentWeatherDataByCityName(request);

                // Cache the result
                await _weatherCacheService.CacheWeatherData(
                    cacheKey,
                    "cityname",
                    weatherData,
                    $"city:{cityName},state:{stateCode ?? "none"},country:{countryCode ?? "none"},units:{units},lang:{lang ?? "none"}");

                return Ok(weatherData);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch weather data for city: {CityName}", cityName);
                return NotFound(new {message = $"Weather data not found for city: {cityName}"});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weather data for city: {CityName}", cityName);
                return BadRequest(new {message = ex.Message});
            }
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
            try
            {
                var request = new CityIdWeatherRequestDto {CityId = cityId, Units = units, Language = lang};

                // Generate cache key
                var cacheKey = _weatherCacheService.GenerateCacheKey(request);

                // Try to get from cache
                var cachedData = await _weatherCacheService.GetCachedWeatherData(cacheKey);
                if (cachedData != null)
                {
                    _logger.LogInformation("Returning cached weather data for city ID: {CityId}", cityId);
                    return Ok(cachedData);
                }

                // Cache miss - fetch from API
                _logger.LogInformation("Cache miss - fetching fresh weather data for city ID: {CityId}", cityId);
                var weatherData = await _currentWeatherData.GetCurrentWeatherDataByCityId(request);

                // Cache the result
                await _weatherCacheService.CacheWeatherData(
                    cacheKey,
                    "cityid",
                    weatherData,
                    $"cityId:{cityId},units:{units},lang:{lang ?? "none"}");

                return Ok(weatherData);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch weather data for city ID: {CityId}", cityId);
                return NotFound(new {message = $"Weather data not found for city ID: {cityId}"});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weather data for city ID: {CityId}", cityId);
                return BadRequest(new {message = ex.Message});
            }
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
            try
            {
                var request = new ZipCodeWeatherRequestDto {ZipCode = zipCode, CountryCode = countryCode, Units = units, Language = lang};

                // Generate cache key
                var cacheKey = _weatherCacheService.GenerateCacheKey(request);

                // Try to get from cache
                var cachedData = await _weatherCacheService.GetCachedWeatherData(cacheKey);
                if (cachedData != null)
                {
                    _logger.LogInformation("Returning cached weather data for ZIP code: {ZipCode}", zipCode);
                    return Ok(cachedData);
                }

                // Cache miss - fetch from API
                _logger.LogInformation("Cache miss - fetching fresh weather data for ZIP code: {ZipCode}", zipCode);
                var weatherData = await _currentWeatherData.GetCurrentWeatherDataByZipCode(request);

                // Cache the result
                await _weatherCacheService.CacheWeatherData(
                    cacheKey,
                    "zipcode",
                    weatherData,
                    $"zip:{zipCode},country:{countryCode},units:{units},lang:{lang ?? "none"}");

                return Ok(weatherData);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch weather data for ZIP code: {ZipCode}", zipCode);
                return NotFound(new {message = $"Weather data not found for ZIP code: {zipCode}"});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weather data for ZIP code: {ZipCode}", zipCode);
                return BadRequest(new {message = ex.Message});
            }
        }
    }
}
