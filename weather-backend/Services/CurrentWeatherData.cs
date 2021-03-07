using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using weather_backend.Models;
using Newtonsoft.Json;

namespace weather_backend.Services
{
    public class CurrentWeatherData
    {
        private HttpClient _httpClient;
        private IConfiguration _configuration;
        private readonly ILogger<CurrentWeatherData> _logger;
        private const string BaseUrl = "https://api.openweathermap.org/data/2.5/weather";

        public CurrentWeatherData(IConfiguration configuration, HttpClient httpClient, ILogger<CurrentWeatherData> logger)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        /**
         * Reference API: https://openweathermap.org/current "By city ID"
         * Return weather information in metric form
         */
        public async Task<WeatherData> GetCurrentWeatherDataByCityId(double cityId)
        {
            string apiKey = _configuration.GetValue<string>("OpenWeatherApiKey");

            UriBuilder uriBuilder = new UriBuilder(BaseUrl);
            NameValueCollection queryCollection = HttpUtility.ParseQueryString(uriBuilder.Query);
            queryCollection["id"] = Convert.ToString(cityId);
            queryCollection["appid"] = apiKey;
            queryCollection["units"] = "metric";
            uriBuilder.Query = queryCollection.ToString();
            string url = uriBuilder.ToString();

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);


            HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage);
            string stringResponse = await response.Content.ReadAsStringAsync(); 

            WeatherData weatherData = JsonConvert.DeserializeObject<WeatherData>(stringResponse);
            return weatherData;
        }
        
    }
}