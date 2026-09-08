using System;
using System.Net;
using System.Net.Http;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SecurityToken;
using Amazon.SimpleSystemsManagement;
using Amazon.Translate;
using ConfigCat.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Polly;
using weather_application;
using weather_backend.Adapters;
using weather_backend.Extensions;
using weather_backend.HostedService;
using weather_backend.Middleware;
using weather_backend.Repository;
using weather_backend.Services;
using weather_backend.Services.Interfaces;
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

        /// <summary>
        /// Retry, circuit breaker and timeouts for the third-party APIs this service calls. Both
        /// upstreams are read-only GETs, so replaying a request is safe.
        /// </summary>
        /// <remarks>
        /// The timings are tighter than the library defaults (30s total / 10s per attempt) because
        /// these calls sit inline in a request the caller is waiting on: four attempts at 4s each
        /// still fits inside the 20s ceiling, so a caller waits at most that long rather than 30s.
        /// The sampling window must stay at or above twice the attempt timeout or the options
        /// validator rejects it at startup.
        /// </remarks>
        private static void ConfigureUpstreamResilience(HttpStandardResilienceOptions options)
        {
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(20);
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(4);

            options.Retry.MaxRetryAttempts = 3;
            options.Retry.Delay = TimeSpan.FromMilliseconds(500);
            options.Retry.BackoffType = DelayBackoffType.Exponential;
            // Jitter matters here because the daily digest fans out over many cities at once; without
            // it a shared upstream blip would put every retry on the same schedule.
            options.Retry.UseJitter = true;

            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.CircuitBreaker.FailureRatio = 0.5;
            options.CircuitBreaker.MinimumThroughput = 10;
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // A size limit is required to stop route-supplied cache keys (see CityList) growing the
            // cache without bound; every entry written to this cache must therefore call SetSize.
            services.AddMemoryCache(options => options.SizeLimit = 1024);
            services.AddHealthChecks();

            // RFC 7807 responses for anything that reaches the top of the pipeline unhandled, plus the
            // bare status codes MVC produces on its own (404 from routing, 400 from model validation).
            services.AddProblemDetails();
            services.AddExceptionHandler<GlobalExceptionHandler>();

            // Order matters at request time, not here: forwarded headers must be applied before the
            // limiter reads the caller's address, otherwise every request looks like the load balancer.
            services.AddForwardedHeadersForLoadBalancer();
            services.AddApiRateLimiting(Configuration);

            services.AddAWSService<IAmazonSecurityTokenService>();

            services.AddControllers();
            services.AddHttpClient();
            services.AddHttpClient<IGeolocationService, GeolocationService>("geolocation")
                .AddStandardResilienceHandler(ConfigureUpstreamResilience);
            services.AddHttpClient<ICurrentWeatherData, CurrentWeatherData>("openweathermap", client =>
                {
                    client.DefaultRequestVersion = HttpVersion.Version20;
                    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
                })
                // The OpenWeatherMap API key travels in the "appid" query parameter, and the default
                // HttpClient loggers write the full request URI at Information level. Suppress them so
                // the key never reaches the logs; CurrentWeatherData logs a secret-free description instead.
                .RemoveAllLoggers()
                .AddStandardResilienceHandler(ConfigureUpstreamResilience);


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
            services.AddApplicationServices(Configuration);
            services.AddSingleton<IDynamoDbClient, DynamoDbClient>();
            // Singleton: EmailService is stateless now that the SmtpClient is created per send, and the
            // singleton Scheduler depends on it.
            services.AddSingleton<EmailService>();
            services.AddTransient<CityList>();
            services.AddSingleton<IWeatherCacheService, WeatherCacheService>();

            services.AddTransient<ThreadExample>();
            services.AddTransient<IAcademicService, AcademicService>();

            services.AddHttpContextAccessor();

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

            //Other registrations
            services.AddStartupTask<WarmupServicesStartupTask>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // First, so everything below sees the caller's real address rather than the load balancer's -
            // the rate limiter partitions on it and GeolocationService reports it. Also feeds
            // UseHttpsRedirection the original scheme.
            app.UseForwardedHeaders();

            // Outside the exception handler on purpose. Its finally block records the status code, and
            // from inside it would run before the handler had turned the exception into a 500 - logging
            // every failed request as though it had succeeded. Out here it also puts the correlation
            // scope around the handler, so the exception record carries the same id as the request.
            app.UseLogMiddleware();

            // Catches everything downstream, including the static file middleware. In Development the
            // developer exception page sits inside it and answers first; everywhere else
            // GlobalExceptionHandler writes the ProblemDetails response.
            app.UseExceptionHandler();

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

            app.UseRouting();

            // After UseRouting so the endpoint is known and per-endpoint [EnableRateLimiting] policies
            // resolve; before the endpoints themselves so rejected requests never reach a controller.
            app.UseRateLimiter();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
