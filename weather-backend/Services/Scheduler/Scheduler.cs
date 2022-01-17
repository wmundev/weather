using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace weather_backend.Services;

public class Scheduler : CronJobService
{
    private readonly IConfiguration _configuration;
    private readonly CurrentWeatherData _currentWeatherData;
    private readonly EmailService _emailService;
    private ILogger<Scheduler> _logger;

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
        var weatherData = await _currentWeatherData.GetCurrentWeatherDataByCityId(cityId);

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