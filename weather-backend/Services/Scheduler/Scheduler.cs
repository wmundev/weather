using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using weather_backend.Models;

namespace weather_backend.Services
{
    public class Scheduler : CronJobService
    {
        private EmailService _emailService;
        private CurrentWeatherData _currentWeatherData;
        private ILogger<Scheduler> _logger;
        private IConfiguration _configuration;

        public Scheduler(EmailService emailService, CurrentWeatherData currentWeatherData, ILogger<Scheduler> logger,
            IConfiguration configuration) :
            base("0 22 * * *", TimeZoneInfo.Utc)
        {
            _emailService = emailService;
            _currentWeatherData = currentWeatherData;
            _logger = logger;
            _configuration = configuration;
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            //melbourne cityid: 7839805
            double cityId = 7839805;
            WeatherData weatherData = await _currentWeatherData.GetCurrentWeatherDataByCityId(cityId);

            _emailService.SendEmail($"{weatherData.name} Current Weather",
                $"Current Temperature: {weatherData.main.temp}, Humidity: {weatherData.main.humidity}",
                _configuration.GetValue<string>("SMTPUsername"));
            // return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}