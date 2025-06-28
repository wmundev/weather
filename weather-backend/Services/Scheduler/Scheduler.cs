using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using weather_backend.Models;
using weather_backend.Services.Interfaces;

namespace weather_backend.Services.Scheduler
{
    public class Scheduler : CronJobService
    {
        private readonly IConfiguration _configuration;
        private readonly ICurrentWeatherData _currentWeatherData;
        private readonly EmailService _emailService;
        private readonly ISecretService _secretService;
        private ILogger<Scheduler> _logger;

        public Scheduler(EmailService emailService, ICurrentWeatherData currentWeatherData, ILogger<Scheduler> logger,
            IConfiguration configuration, ISecretService secretService) :
            base(Constants.CRON_EXPRESSION_SCHEDULE_JOB, TimeZoneInfo.Utc)
        {
            _emailService = emailService;
            _currentWeatherData = currentWeatherData;
            _logger = logger;
            _configuration = configuration;
            _secretService = secretService;
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing schedule");
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
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
