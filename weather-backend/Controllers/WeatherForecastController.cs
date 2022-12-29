using System;
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
        private readonly ICurrentWeatherData _currentWeatherData;
        private readonly EmailService _emailService;
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ISecretService _secretService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration,
            ICurrentWeatherData currentWeatherData, EmailService emailService, CityList cityList, ISecretService secretService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _currentWeatherData = currentWeatherData ?? throw new ArgumentNullException(nameof(currentWeatherData));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _cityList = cityList ?? throw new ArgumentNullException(nameof(cityList));
            _secretService = secretService ?? throw new ArgumentNullException(nameof(secretService));
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

            var receiverEmail = await _secretService.FetchSpecificSecret(nameof(AllSecrets.SMTPUsername));
            if (receiverEmail is null)
            {
                throw new Exception("Receiver email in secret is null");
            }

            _emailService.SendEmail($"{weatherData.name} Current Weather",
                $"Current Temperature: {weatherData.main.temp}, Humidity: {weatherData.main.humidity}",
                receiverEmail);
            return weatherData;
        }

        [HttpGet]
        [Route("/city/all")]
        public ActionResult<IEnumerable<City>> GetCities([FromQuery(Name = "num")] int numberOfCities = 100)
        {
            if (numberOfCities > 500)
                return BadRequest(new ProblemDetails { Type = "too large", Detail = "size is too large" });

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
