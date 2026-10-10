using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using weather_backend.Services.Interfaces;

namespace weather_backend.Services.Scheduler
{
    public class Scheduler : CronJobService
    {
        private readonly ICurrentWeatherData _currentWeatherData;
        private readonly EmailService _emailService;
        private readonly ILogger<Scheduler> _logger;
        private readonly ISecretService _secretService;

        public Scheduler(EmailService emailService, ICurrentWeatherData currentWeatherData, ILogger<Scheduler> logger,
            ISecretService secretService) :
            base(Constants.CRON_EXPRESSION_SCHEDULE_JOB, TimeZoneInfo.Utc, logger)
        {
            _emailService = emailService;
            _currentWeatherData = currentWeatherData;
            _logger = logger;
            _secretService = secretService;
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing schedule");
            var weatherData = await _currentWeatherData.GetCurrentWeatherDataByCityId(Constants.DEFAULT_CITY_ID, cancellationToken);
            // The digest goes to the sending account itself.
            var receiverEmail = (await _secretService.GetSecrets()).SMTPUsername;

            await _emailService.SendEmail($"{weatherData.name} Current Weather",
                $"Current Temperature: {weatherData.main.temp}, Humidity: {weatherData.main.humidity}",
                receiverEmail);
        }
    }
}
