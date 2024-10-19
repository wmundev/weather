using System;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SecurityToken;
using Amazon.SimpleSystemsManagement;
using Amazon.Translate;
using ConfigCat.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using weather_backend.Adapters;
using weather_backend.Extensions;
using weather_backend.HostedService;
using weather_backend.Middleware;
using weather_backend.Repository;
using weather_backend.Services;
using weather_backend.Services.Scheduler;
using weather_backend.StartupTask;
using weather_repository;

namespace weather_backend
{
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
            services.AddMemoryCache();
            services.AddHealthChecks();

            services.AddAWSService<IAmazonSecurityTokenService>();

            services.AddControllers();
            services.AddHttpClient();
            services.AddHttpClient<IGeolocationService, GeolocationService>("geolocation");
            services.AddHttpClient<ICurrentWeatherData, CurrentWeatherData>("openweathermap");

            services.AddAutoMapper(typeof(Startup));

            var configCatSdkKey = Configuration.GetValue<string>("ConfigCat:Key");
            if (string.IsNullOrEmpty(configCatSdkKey))
            {
                throw new InvalidOperationException("ConfigCat SDK Key is missing");
            }

            services.AddSingleton<IConfigCatClient>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ConfigCatClient>>();
                return ConfigCatClient.Get(configCatSdkKey, options =>
                {
                    options.PollingMode = PollingModes.LazyLoad(cacheTimeToLive: TimeSpan.FromSeconds(600));
                    options.Logger = new ConfigCatToMSLoggerAdapter(logger);
                });
            });


            var dynamodbLocalMode = Configuration.GetValue("DynamoDb:LocalMode", false);
            if (dynamodbLocalMode)
            {
                services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
                var awsOptions = new AWSOptions {DefaultClientConfig = {ServiceURL = "http://localhost:8000"}};
                services.AddAWSService<IAmazonDynamoDB>(awsOptions);
                services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
            }
            else
            {
                services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
                // TODO uncomment for caching aws creds
                // services.AddSingleton<AmazonCredentialsCachingService>();
                // TODO uncomment for caching aws creds
                // services.AddSingleton<IDynamoDBContext>(provider =>
                // {
                //     var awsOptions = new AWSOptions();
                //     awsOptions.Credentials = provider.GetRequiredService<AmazonCredentialsCachingService>();
                //
                //     var dynamodbClient = awsOptions.CreateServiceClient<IAmazonDynamoDB>();
                //     DynamoDBContext context = new DynamoDBContext(dynamodbClient);
                //     return context;
                // });
                services.AddAWSService<IAmazonDynamoDB>();
                services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
            }

            //TODO add back redis if needed
            // if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != Environments.Development)
            //     try
            //     {
            //         var multiplexer = ConnectionMultiplexer.Connect(new ConfigurationOptions { EndPoints = { "redis-test-unenc.fhjziy.ng.0001.use1.cache.amazonaws.com:6379" }, ConnectRetry = 5 });
            //         services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            //     }
            //     catch (RedisConnectionException e)
            //     {
            //         // we add redis as optional and not fail if cannot connect
            //         Console.WriteLine(e.Message);
            //         Console.WriteLine(e.StackTrace);
            //     }
            // else
            //     try
            //     {
            //         var multiplexer = ConnectionMultiplexer.Connect(new ConfigurationOptions { EndPoints = { "redis-test-unenc.fhjziy.ng.0001.use1.cache.amazonaws.com:6379" }, ConnectRetry = 5 });
            //         services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            //     }
            //     catch (RedisConnectionException e)
            //     {
            //         // we add redis as optional and not fail if cannot connect
            //         Console.WriteLine(e.Message);
            //         Console.WriteLine(e.StackTrace);
            //     }

            services.AddAWSService<IAmazonSimpleSystemsManagement>();
            services.AddAWSService<IAmazonTranslate>();
            services.AddSingleton<ILanguageTranslatorService, LanguageTranslatorService>();

            services.AddInfrastructureServices(Configuration);
            services.AddSingleton<IDynamoDbClient, DynamoDbClient>();
            services.AddTransient<EmailService>();
            services.AddTransient<CityList>();

            services.AddTransient<ThreadExample>();
            services.AddTransient<AcademicService>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddSingleton<IPhoneService, PhoneService>();
            services.AddSingleton<DelegateService>();

            services.AddSingleton<SecretMemoryCache>();
            services.AddSingleton<ISecretService, SecretService>();
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue>(_ =>
            {
                var queueCapacity = 100;
                return new DefaultBackgroundTaskQueue(queueCapacity);
            });

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
            // services.AddSingleton<IKafkaProducer, KafkaProducer>();
            services.AddHostedService<Scheduler>();
            // services.AddHostedService<KafkaHostedService>();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "weather_backend", Version = "v1"}); });
            services.AddSingleton<IEncryptionService, EncryptionService>();

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

            if (!env.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

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
}
