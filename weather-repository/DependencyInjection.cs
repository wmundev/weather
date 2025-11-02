using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using weather_repository.City;

namespace weather_repository
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ICityRepository, CityRepository>();
            services.AddSingleton<IWeatherCacheRepository, WeatherCacheRepository>();
            return services;
        }
    }
}
