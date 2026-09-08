using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace weather_backend.HostedService
{
    public sealed class QueuedHostedService : BackgroundService
    {
        private readonly ILogger<QueuedHostedService> _logger;
        private readonly IBackgroundTaskQueue _taskQueue;
        private int count;

        public QueuedHostedService(
            IBackgroundTaskQueue taskQueue,
            ILogger<QueuedHostedService> logger)
        {
            (_taskQueue, _logger) = (taskQueue, logger);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // The service name is a parameter rather than part of the template: an interpolated string
            // arrives at the provider already flattened, leaving nothing structured to filter on.
            _logger.LogInformation("{ServiceName} is running", nameof(QueuedHostedService));

            return ProcessTaskQueueAsync(stoppingToken);
        }

        private async Task ProcessTaskQueueAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
                try
                {
                    var workItem =
                        await _taskQueue.DequeueAsync(stoppingToken);

                    _logger.LogDebug("Dequeued a background work item");
                    await workItem(stoppingToken);
                    count += 1;
                    _logger.LogDebug("Completed background work item {CompletedCount}", count);
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stoppingToken was signaled
                    _logger.LogInformation("Background queue processing cancelled during shutdown");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing task work item.");
                }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{ServiceName} is stopping", nameof(QueuedHostedService));

            await _taskQueue.CompletionAsync();

            _logger.LogInformation("All queued work items flushed after {CompletedCount} completed", count);

            await base.StopAsync(stoppingToken);
        }
    }
}
