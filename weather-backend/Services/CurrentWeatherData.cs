using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using weather_backend.Dto;
using weather_backend.Models;
using weather_backend.Services.Interfaces;

namespace weather_backend.Services
{
    public class CurrentWeatherData : ICurrentWeatherData
    {
        private const string BaseUrl = "https://api.openweathermap.org/data/2.5/weather";
        private readonly HttpClient _httpClient;
        private readonly ILogger<CurrentWeatherData> _logger;
        private readonly ISecretService _secretService;

        public CurrentWeatherData(HttpClient httpClient, ILogger<CurrentWeatherData> logger, ISecretService secretService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _secretService = secretService ?? throw new ArgumentNullException(nameof(secretService));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Legacy method - Get weather data by city ID in metric units
        /// </summary>
        public Task<WeatherData> GetCurrentWeatherDataByCityId(double cityId, CancellationToken cancellationToken = default)
        {
            return GetCurrentWeatherDataByCityId(new CityIdWeatherRequestDto {CityId = cityId, Units = WeatherUnit.Metric}, cancellationToken);
        }

        /// <summary>
        /// Get current weather data by city ID with custom units and language
        /// </summary>
        public Task<WeatherData> GetCurrentWeatherDataByCityId(CityIdWeatherRequestDto request, CancellationToken cancellationToken = default)
        {
            var cityId = request.CityId.ToString(CultureInfo.InvariantCulture);

            return FetchWeatherData(new() {["id"] = cityId}, request.Units, request.Language, $"cityId={cityId}", cancellationToken);
        }

        /// <summary>
        /// Get current weather data by geographic coordinates
        /// </summary>
        public Task<WeatherData> GetCurrentWeatherDataByCoordinates(CoordinatesWeatherRequestDto request, CancellationToken cancellationToken = default)
        {
            var latitude = request.Latitude.ToString(CultureInfo.InvariantCulture);
            var longitude = request.Longitude.ToString(CultureInfo.InvariantCulture);

            return FetchWeatherData(new() {["lat"] = latitude, ["lon"] = longitude}, request.Units, request.Language,
                $"lat={latitude},lon={longitude}", cancellationToken);
        }

        /// <summary>
        /// Get current weather data by city name
        /// </summary>
        public Task<WeatherData> GetCurrentWeatherDataByCityName(CityNameWeatherRequestDto request, CancellationToken cancellationToken = default)
        {
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

            return FetchWeatherData(new() {["q"] = queryValue}, request.Units, request.Language, $"q={queryValue}", cancellationToken);
        }

        /// <summary>
        /// Get current weather data by ZIP/postal code
        /// </summary>
        public Task<WeatherData> GetCurrentWeatherDataByZipCode(ZipCodeWeatherRequestDto request, CancellationToken cancellationToken = default)
        {
            var zipQuery = $"{request.ZipCode},{request.CountryCode}";

            return FetchWeatherData(new() {["zip"] = zipQuery}, request.Units, request.Language, $"zip={zipQuery}", cancellationToken);
        }

        /// <summary>
        /// Build URL with query parameters
        /// </summary>
        private static string BuildUrl(Dictionary<string, string> parameters, string? language = null)
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
        /// Fetch weather data from OpenWeatherMap API.
        /// </summary>
        /// <param name="locationParameters">The query parameters that select the location.</param>
        /// <param name="units">Units of measurement for the response.</param>
        /// <param name="language">Optional output language.</param>
        /// <param name="queryDescription">A secret-free description of the query, safe to log.</param>
        /// <param name="cancellationToken">Cancels the upstream call, for example when the caller disconnects.</param>
        private async Task<WeatherData> FetchWeatherData(Dictionary<string, string> locationParameters, WeatherUnit units, string? language,
            string queryDescription, CancellationToken cancellationToken)
        {
            locationParameters["appid"] = await GetApiKey();
            locationParameters["units"] = GetUnitsString(units);

            // Never logged - the URL carries the API key in the appid parameter.
            var url = BuildUrl(locationParameters, language);

            _logger.LogInformation("Fetching weather data from OpenWeatherMap for {Query}", queryDescription);

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            // Default options, not the web defaults ReadFromJsonAsync would otherwise pick: the models bind by
            // their exact JSON names.
            return await response.Content.ReadFromJsonAsync<WeatherData>(JsonSerializerOptions.Default, cancellationToken)
                   ?? throw new InvalidOperationException("OpenWeatherMap returned an empty weather document.");
        }

        private async Task<string> GetApiKey()
        {
            var apiKey = await _secretService.FetchSpecificSecret(nameof(AllSecrets.OpenWeatherApiKey));

            return string.IsNullOrEmpty(apiKey)
                ? throw new InvalidOperationException("OpenWeatherMap API key is not configured.")
                : apiKey;
        }

        /// <summary>
        /// Convert WeatherUnit enum to API string
        /// </summary>
        private static string GetUnitsString(WeatherUnit unit)
        {
            return unit switch
            {
                WeatherUnit.Metric => "metric",
                WeatherUnit.Imperial => "imperial",
                WeatherUnit.Standard => "standard",
                _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, "Unsupported weather unit.")
            };
        }
    }
}
