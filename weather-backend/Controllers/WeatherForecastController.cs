using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using weather_backend.Dto;
using weather_backend.Models;
using weather_backend.Services;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly CityList _cityList;
        private readonly IConfiguration _configuration;
        private readonly CurrentWeatherData _currentWeatherData;
        private readonly EmailService _emailService;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration,
            CurrentWeatherData currentWeatherData, EmailService emailService, CityList cityList)
        {
            _logger = logger;
            _configuration = configuration;
            _currentWeatherData = currentWeatherData;
            _emailService = emailService;
            _cityList = cityList;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/weather")]
        public async Task<WeatherData> GetCurrentWeatherDataById()
        {
            //melbourne cityid: 7839805
            double cityId = 7839805;
            var weatherData = await _currentWeatherData.GetCurrentWeatherDataByCityId(cityId);

            _emailService.SendEmail($"{weatherData.name} Current Weather",
                $"Current Temperature: {weatherData.main.temp}, Humidity: {weatherData.main.humidity}",
                _configuration.GetValue<string>("SMTPUsername"));
            return weatherData;
        }

        [HttpGet]
        [Route("/city/all")]
        public async Task<ActionResult<IEnumerable<City>>> GetCities([FromQuery(Name = "num")] int numberOfCities = 100)
        {
            if (numberOfCities > 500)
                return BadRequest(new ProblemDetails {Type = "too large", Detail = "size is too large"});

            var allCitiesInAustralia = _cityList.GetAllCitiesInAustralia().Take(numberOfCities);
            return Ok(allCitiesInAustralia);
        }

        [HttpGet]
        [Route("/populate")]
        public async Task<ActionResult<int>> PopulateDynamoDbDatabase()
        {
            await _cityList.PopulateDynamoDbDatabase();
            return Ok(1);
        }

        [HttpGet]
        [Route("/city/{name}")]
        public async Task<ActionResult<DynamoDbCity>> GetCityInformation(string name)
        {
            var city = await _cityList.GetCityInfo(name);
            return Ok(city);
        }
    }
}