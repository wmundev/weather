using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using weather_backend.Models;

namespace weather_backend.Services
{
    public class CurrentWeatherData
    {
        private const string BaseUrl = "https://api.openweathermap.org/data/2.5/weather";
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<CurrentWeatherData> _logger;
        private readonly ISecretService _secretService;

        public CurrentWeatherData(IConfiguration configuration, HttpClient httpClient, ILogger<CurrentWeatherData> logger, ISecretService secretService)
        {
            _logger = logger;
            _secretService = secretService;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        /**
         * Reference API: https://openweathermap.org/current "By city ID"
         * Return weather information in metric form
         */
        public async Task<WeatherData> GetCurrentWeatherDataByCityId(double cityId)
        {
            var apiKey = await _secretService.FetchSpecificSecret(nameof(AllSecrets.OpenWeatherApiKey));

            var uriBuilder = new UriBuilder(BaseUrl);
            var queryCollection = HttpUtility.ParseQueryString(uriBuilder.Query);
            queryCollection["id"] = Convert.ToString(cityId);
            queryCollection["appid"] = apiKey;
            queryCollection["units"] = "metric";
            uriBuilder.Query = queryCollection.ToString();
            var url = uriBuilder.ToString();

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);


            var response = await _httpClient.SendAsync(httpRequestMessage);
            var stringResponse = await response.Content.ReadAsStringAsync();

            var weatherData = JsonSerializer.Deserialize<WeatherData>(stringResponse);
            return weatherData;
        }
    }
}