using System.Net.Http;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using weather_backend.Controllers;
using weather_backend.Extensions;
using weather_backend.Middleware;
using weather_backend.Repository;
using weather_backend.Services;
using weather_backend.StartupTask;

namespace weather_backend;

public class Startup
{
    private IWebHostEnvironment _webHostEnvironment;

    public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        Configuration = configuration;
        _webHostEnvironment = webHostEnvironment;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHealthChecks();

        services.AddControllers();
        services.AddHttpClient();

        services.AddAutoMapper(typeof(Startup));

        var dynamodbLocalMode = Configuration.GetValue("DynamoDb:LocalMode", false);
        if (dynamodbLocalMode)
        {
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            var awsOptions = new AWSOptions
            {
                DefaultClientConfig =
                {
                    ServiceURL = "http://localhost:8000"
                }
            };
            services.AddAWSService<IAmazonDynamoDB>(awsOptions);
            services.AddTransient<IDynamoDBContext, DynamoDBContext>();
        }
        else
        {
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddTransient<IDynamoDBContext, DynamoDBContext>();
        }

        services.AddTransient<IDynamoDbClient, DynamoDbClient>();
        services.AddTransient<EmailService>();
        services.AddTransient<CurrentWeatherData>();
        services.AddTransient<HttpClient>();
        services.AddTransient<WeatherForecastController>();
        services.AddTransient<CityList>();

        services.AddTransient<ThreadExample>();
        services.AddTransient<AcademicService>();

        services.AddTransient<GeolocationService, GeolocationService>();
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

        services.AddSingleton<IPhoneService, PhoneService>();
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
        services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "weather_backend", Version = "v1"}); });

        //Other registrations
        services
            .AddStartupTask<WarmupServicesStartupTask>()
            .TryAddSingleton(services);
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

        app.UseStaticFiles();

        app.UseLogMiddleware();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health");
        });
    }
}