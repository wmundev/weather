using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using weather_backend.Services;
using weather_backend.Services.Interfaces;
using weather_repository.City;

namespace weather_backend.StartupTask
{
    public class WarmupServicesStartupTask : IStartupTask
    {
        /// <summary>
        /// Services worth paying the construction cost for at boot rather than on the first request.
        /// This is deliberately an allow-list: resolving the whole container instead pulled in every
        /// AWS client and every service that talks to the network during startup.
        /// </summary>
        private static readonly Type[] ServicesToWarmUp =
        {
            typeof(ISecretService),
            typeof(IWeatherCacheService),
            typeof(ICityRepository)
        };

        private readonly ILogger<WarmupServicesStartupTask> _logger;
        private readonly IServiceProvider _provider;

        public WarmupServicesStartupTask(IServiceProvider provider, ILogger<WarmupServicesStartupTask> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var scope = _provider.CreateScope();

            foreach (var serviceType in ServicesToWarmUp)
                try
                {
                    scope.ServiceProvider.GetServices(serviceType);
                }
                catch (Exception exception)
                {
                    // Warmup is an optimisation; a failure here must not stop the host from starting.
                    _logger.LogWarning(exception, "Failed to warm up {ServiceType}", serviceType.Name);
                }

            return Task.CompletedTask;
        }
    }
}