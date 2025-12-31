using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using weather_backend.Dto;
using weather_backend.Exceptions;
using weather_backend.Models;
using weather_backend.Services.Interfaces;

namespace weather_backend.Services
{
    public class CurrentWeatherData : ICurrentWeatherData
    {
        private const string BaseUrl = "https://api.openweathermap.org/data/2.5/weather";
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<CurrentWeatherData> _logger;
        private readonly ISecretService _secretService;

        public CurrentWeatherData(IConfiguration configuration, HttpClient httpClient, ILogger<CurrentWeatherData> logger, ISecretService secretService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _secretService = secretService ?? throw new ArgumentNullException(nameof(secretService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Legacy method - Get weather data by city ID in metric units
        /// </summary>
        public async Task<WeatherData> GetCurrentWeatherDataByCityId(double cityId)
        {
            var request = new CityIdWeatherRequestDto {CityId = cityId, Units = WeatherUnit.Metric};
            return await GetCurrentWeatherDataByCityId(request);
        }

        /// <summary>
        /// Get current weather data by city ID with custom units and language
        /// </summary>
        public async Task<WeatherData> GetCurrentWeatherDataByCityId(CityIdWeatherRequestDto request)
        {
            var apiKey = await _secretService.FetchSpecificSecret(nameof(AllSecrets.OpenWeatherApiKey));
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ConfigurationException("OpenWeatherMap API key is not configured");
            }

            var url = BuildUrl(new() {["id"] = Convert.ToString(request.CityId), ["appid"] = apiKey, ["units"] = GetUnitsString(request.Units)}, request.Language);

            return await FetchWeatherData(url);
        }

        /// <summary>
        /// Get current weather data by geographic coordinates
        /// </summary>
        public async Task<WeatherData> GetCurrentWeatherDataByCoordinates(CoordinatesWeatherRequestDto request)
        {
            var apiKey = await _secretService.FetchSpecificSecret(nameof(AllSecrets.OpenWeatherApiKey));
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ConfigurationException("OpenWeatherMap API key is not configured");
            }

            var url = BuildUrl(new() {["lat"] = request.Latitude.ToString(), ["lon"] = request.Longitude.ToString(), ["appid"] = apiKey, ["units"] = GetUnitsString(request.Units)}, request.Language);

            return await FetchWeatherData(url);
        }

        /// <summary>
        /// Get current weather data by city name
        /// </summary>
        public async Task<WeatherData> GetCurrentWeatherDataByCityName(CityNameWeatherRequestDto request)
        {
            var apiKey = await _secretService.FetchSpecificSecret(nameof(AllSecrets.OpenWeatherApiKey));
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ConfigurationException("OpenWeatherMap API key is not configured");
            }

            // Build query string: cityName or cityName,countryCode or cityName,stateCode,countryCode
            var queryValue = request.CityName;
            if (!string.IsNullOrEmpty(request.StateCode) && !string.IsNullOrEmpty(request.CountryCode))
            {
                queryValue = $"{request.CityName},{request.StateCode},{request.CountryCode}";
            }
            else if (!string.IsNullOrEmpty(request.CountryCode))
            {
                queryValue = $"{request.CityName},{request.CountryCode}";
            }

            var url = BuildUrl(new() {["q"] = queryValue, ["appid"] = apiKey, ["units"] = GetUnitsString(request.Units)}, request.Language);

            return await FetchWeatherData(url);
        }

        /// <summary>
        /// Get current weather data by ZIP/postal code
        /// </summary>
        public async Task<WeatherData> GetCurrentWeatherDataByZipCode(ZipCodeWeatherRequestDto request)
        {
            var apiKey = await _secretService.FetchSpecificSecret(nameof(AllSecrets.OpenWeatherApiKey));
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ConfigurationException("OpenWeatherMap API key is not configured");
            }

            var zipQuery = $"{request.ZipCode},{request.CountryCode}";

            var url = BuildUrl(new() {["zip"] = zipQuery, ["appid"] = apiKey, ["units"] = GetUnitsString(request.Units)}, request.Language);

            return await FetchWeatherData(url);
        }

        /// <summary>
        /// Build URL with query parameters
        /// </summary>
        private string BuildUrl(Dictionary<string, string> parameters, string? language = null)
        {
            var uriBuilder = new UriBuilder(BaseUrl);
            var queryCollection = HttpUtility.ParseQueryString(uriBuilder.Query);

            foreach (var param in parameters)
            {
                queryCollection[param.Key] = param.Value;
            }

            if (!string.IsNullOrEmpty(language))
            {
                queryCollection["lang"] = language;
            }

            uriBuilder.Query = queryCollection.ToString();
            return uriBuilder.ToString();
        }

        /// <summary>
        /// Fetch weather data from OpenWeatherMap API
        /// </summary>
        private async Task<WeatherData> FetchWeatherData(string url)
        {
            _logger.LogInformation("Fetching weather data from: {Url}", url);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _httpClient.SendAsync(httpRequestMessage);

            response.EnsureSuccessStatusCode();

            var stringResponse = await response.Content.ReadAsStringAsync();
            var weatherData = JsonSerializer.Deserialize<WeatherData>(stringResponse);

            if (weatherData == null)
            {
                throw new InvalidOperationException("Failed to deserialize weather data");
            }

            return weatherData;
        }

        /// <summary>
        /// Convert WeatherUnit enum to API string
        /// </summary>
        private string GetUnitsString(WeatherUnit unit)
        {
            return unit switch
            {
                WeatherUnit.Metric => "metric",
                WeatherUnit.Imperial => "imperial",
                WeatherUnit.Standard => "standard",
                _ => "metric"
            };
        }
    }
}
