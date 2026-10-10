using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using weather_repository.City;
using weather_repository.DynamoDb;

namespace weather_repository
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ICityRepository, CityRepository>();
            services.AddSingleton<IDynamoDbClient, DynamoDbClient>();
            return services;
        }
    }
}
