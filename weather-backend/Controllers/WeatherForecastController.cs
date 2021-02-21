using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using weather_backend.Models;
using weather_backend.Services;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private CurrentWeatherData _currentWeatherData;
        private IConfiguration _configuration;
        private EmailService _emailService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _currentWeatherData = new CurrentWeatherData(configuration, new HttpClient());
            _emailService = new EmailService(configuration);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/weather")]
        public async Task<WeatherData> GetCurrentWeatherDataById()
        {
            //melbourne cityid: 7839805
            double cityId = 7839805;
            WeatherData weatherData = await _currentWeatherData.GetCurrentWeatherDataByCityId(cityId);

            _emailService.SendEmail($"{weatherData.name} Current Weather",
                $"Current Temperature: {weatherData.main.temp}, Humidity: {weatherData.main.humidity}",
                _configuration.GetValue<string>("SMTPUsername"));
            return weatherData;
        }
    }
}