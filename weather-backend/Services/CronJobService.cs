using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace weather_backend.Services
{
    /// <summary>
    /// Runs <see cref="DoWork"/> on a cron schedule for as long as the host is running.
    /// </summary>
    public abstract class CronJobService : BackgroundService
    {
        private readonly CronExpression _expression;
        private readonly ILogger _logger;
        private readonly TimeZoneInfo _timeZoneInfo;

        protected CronJobService(string cronExpression, TimeZoneInfo timeZoneInfo, ILogger logger)
        {
            _expression = CronExpression.Parse(cronExpression);
            _timeZoneInfo = timeZoneInfo;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var jobName = GetType().Name;

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTimeOffset.UtcNow;
                var next = _expression.GetNextOccurrence(now, _timeZoneInfo);
                if (next is null)
                {
                    _logger.LogWarning("Cron expression for {JobName} has no further occurrences; the job will not run again", jobName);
                    return;
                }

                var delay = next.Value - now;
                if (delay > TimeSpan.Zero)
                {
                    try
                    {
                        await Task.Delay(delay, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }

                if (stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    await DoWork(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception exception)
                {
                    // A failing job must not bring the host down - log it and wait for the next occurrence.
                    _logger.LogError(exception, "Scheduled job {JobName} failed", jobName);
                }
            }
        }

        public virtual Task DoWork(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
