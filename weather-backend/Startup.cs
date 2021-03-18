using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using weather_backend.Controllers;
using weather_backend.Services;

namespace weather_backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHttpClient();
            
            services.AddTransient<EmailService>();
            services.AddTransient<CurrentWeatherData>();
            services.AddTransient<HttpClient>();
            services.AddTransient<WeatherForecastController>();
            services.AddTransient<CityList>();

            services.AddTransient<ThreadExample>();
            services.AddTransient<AcademicService>();
            //TODO: doesn't work, will break email sending
            // services.AddTransient<SmtpClient>((serviceProvider) =>
            // {
            //     var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            //     string emailUsername = configuration.GetValue<string>("SMTPUsername");
            //     string emailPassword = configuration.GetValue<string>("SMTPPassword");
            //     string emailHost = configuration.GetValue<string>("SMTPHost");
            //     int emailPort = configuration.GetValue<int>("SMTPPort");
            //     return new SmtpClient()
            //     {
            //         Host = emailHost,
            //         Port = emailPort,
            //         Credentials = new NetworkCredential(emailUsername, emailPassword),
            //         EnableSsl = true
            //     };
            // });
            
            services.AddHostedService<Scheduler>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "weather_backend", Version = "v1"});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "weather_backend v1"));
            }

            app.UseHttpsRedirection();

            // app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}