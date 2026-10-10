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
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Npgsql;
using Polly;
using StackExchange.Redis;
using weather_application;
using weather_backend.Adapters;
using weather_backend.Extensions;
using weather_backend.HostedService;
using weather_backend.Middleware;
using weather_backend.Services;
using weather_backend.Services.Interfaces;
using weather_backend.Services.Scheduler;
using weather_backend.StartupTask;
using weather_repository;
using weather_repository.DynamoDb;

namespace weather_backend.Hosting
{
    /// <summary>
    /// Service registration for the API, called from Program.cs.
    /// </summary>
    public static class WeatherApiServices
    {
        /// <summary>
        /// Upper bound, in approximate bytes, on the shared IMemoryCache.
        /// </summary>
        public const long SharedMemoryCacheSizeLimitBytes = 16 * 1024 * 1024;

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

        public static IServiceCollection AddWeatherApi(this IServiceCollection services, IConfiguration configuration)
        {
            // A size limit is required to stop route-supplied cache keys (CityList, the weather queries)
            // growing the cache without bound; every entry written to this cache must therefore call SetSize.
            // Sizes are approximate bytes: HybridCache, which keeps the weather responses here, sizes each
            // entry by its serialized payload, so an entry-count limit would hold only a handful of them.
            services.AddMemoryCache(options => options.SizeLimit = SharedMemoryCacheSizeLimitBytes);
            services.AddHealthChecks();

            // RFC 7807 responses for anything that reaches the top of the pipeline unhandled, plus the
            // bare status codes MVC produces on its own (404 from routing, 400 from model validation).
            services.AddProblemDetails();
            services.AddExceptionHandler<GlobalExceptionHandler>();

            // Order matters at request time, not here: forwarded headers must be applied before the
            // limiter reads the caller's address, otherwise every request looks like the load balancer.
            services.AddForwardedHeadersForLoadBalancer();
            services.AddApiRateLimiting(configuration);

            services.AddAWSService<IAmazonSecurityTokenService>();

            services.AddControllers();
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

            var configCatSdkKey = configuration.GetValue<string>("ConfigCat:Key");
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

            services.AddDefaultAWSOptions(configuration.GetAWSOptions());

            // Local mode points DynamoDB alone at the container in database/; every other AWS client
            // keeps the default options.
            if (configuration.GetValue("DynamoDb:LocalMode", false))
            {
                services.AddAWSService<IAmazonDynamoDB>(new AWSOptions { DefaultClientConfig = { ServiceURL = "http://localhost:8000" } });
            }
            else
            {
                services.AddAWSService<IAmazonDynamoDB>();
            }

            // AWSSDK v4 obsoletes the DynamoDBContext constructors in favour of the builder.
            services.AddSingleton<IDynamoDBContext>(serviceProvider => new DynamoDBContextBuilder()
                .WithDynamoDBClient(serviceProvider.GetRequiredService<IAmazonDynamoDB>)
                .Build());

            // Redis is optional. With no connection string nothing is registered, and NewFeatureController
            // answers 503 on its Redis routes instead of failing to construct - which used to take its
            // Redis-free routes down with them.
            var redisConnectionString = configuration.GetValue<string>("Redis:ConnectionString");
            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                services.AddSingleton<IConnectionMultiplexer>(_ =>
                {
                    var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
                    // Keep retrying in the background rather than throwing if Redis is down when first
                    // used; commands fail on their own until it is reachable.
                    redisOptions.AbortOnConnectFail = false;
                    return ConnectionMultiplexer.Connect(redisOptions);
                });
            }

            services.AddAWSService<IAmazonSimpleSystemsManagement>();
            services.AddAWSService<IAmazonTranslate>();
            services.AddSingleton<ILanguageTranslatorService, LanguageTranslatorService>();

            services.AddInfrastructureServices(configuration);
            services.AddApplicationServices(configuration);
            // Singleton: EmailService is stateless now that the SmtpClient is created per send, and the
            // singleton Scheduler depends on it.
            services.AddSingleton<EmailService>();
            services.AddTransient<CityList>();
            // HybridCache keeps its in-process tier in the shared IMemoryCache registered above.
            services.AddHybridCache();
            services.AddSingleton<IWeatherCacheService, WeatherCacheService>();

            // Postgres backs /academic only and is optional, like Redis: with no connection string nothing is
            // registered and AcademicController answers 503. NpgsqlDataSource owns the connection pool, so it
            // is a singleton; the container disposes it on shutdown.
            var academicConnectionString = configuration.GetConnectionString("Academic");
            if (!string.IsNullOrWhiteSpace(academicConnectionString))
            {
                services.AddSingleton(_ => NpgsqlDataSource.Create(academicConnectionString));
                services.AddSingleton<IAcademicService, AcademicService>();
            }

            services.AddHttpContextAccessor();

            services.AddSingleton<IPhoneService, PhoneService>();

            services.AddSingleton<SecretMemoryCache>();
            services.AddSingleton<ISecretService, SecretService>();
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue>(_ =>
            {
                var queueCapacity = 100;
                return new DefaultBackgroundTaskQueue(queueCapacity);
            });

            // services.AddSingleton<IKafkaProducer, KafkaProducer>();
            services.AddHostedService<Scheduler>();
            // services.AddHostedService<KafkaHostedService>();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "weather_backend", Version = "v1" }); });

            services.AddStartupTask<WarmupServicesStartupTask>();

            return services;
        }
    }
}
