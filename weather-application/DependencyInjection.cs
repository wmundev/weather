using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using weather_application.Services;
using weather_application.Services.Interfaces;

namespace weather_application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IEncryptionService, EncryptionService>();
            return services;
        }
    }
}
