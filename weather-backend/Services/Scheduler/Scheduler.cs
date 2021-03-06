using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace weather_backend.Services
{
    public class Scheduler : CronJobService
    {
        private EmailService _emailService;
        private CurrentWeatherData _currentWeatherData;
        private ILogger<Scheduler> _logger;

        public Scheduler(EmailService emailService, CurrentWeatherData currentWeatherData, ILogger<Scheduler> logger) :
            base("0 22 * * *", TimeZoneInfo.Utc)
        {
            _emailService = emailService;
            _currentWeatherData = currentWeatherData;
            _logger = logger;
        }

        public override Task DoWork(CancellationToken cancellationToken)
        {
            _logger.Log(LogLevel.Information, "wow crazy man");
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

    }
}