using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using weather_backend.Configuration;

// Both Microsoft.AspNetCore.HttpOverrides and System.Net define IPNetwork; KnownIPNetworks takes the latter.
using IPNetwork = System.Net.IPNetwork;

namespace weather_backend.Extensions
{
    public static class RateLimitingExtensions
    {
        /// <summary>
        /// Policy name for endpoints that bypass the weather cache and therefore spend OpenWeatherMap
        /// quota on every single call.
        /// </summary>
        public const string UncachedPolicy = "uncached";

        /// <summary>
        /// Paths that must never be throttled. The ECS/ALB health check polls /health continuously from
        /// one address; rate limiting it would take healthy tasks out of service under load, which is
        /// exactly when the limiter matters most.
        /// </summary>
        private static readonly string[] ExemptPaths = { "/health" };

        /// <summary>
        /// Trusts the X-Forwarded-For that the Application Load Balancer sets, so the limiter partitions
        /// on the real caller.
        /// </summary>
        /// <remarks>
        /// Without this every request appears to come from the load balancer's private address, and a
        /// per-IP limiter degrades into one shared bucket that any single caller can exhaust for
        /// everyone. Only private peers are trusted, so a public client cannot forge the header: the ALB
        /// appends the real client address to the right of whatever the caller sent, and a ForwardLimit
        /// of 1 reads only that rightmost entry.
        /// </remarks>
        public static IServiceCollection AddForwardedHeadersForLoadBalancer(this IServiceCollection services)
        {
            return services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.ForwardLimit = 1;

                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
                foreach (var network in new[]
                         {
                             new IPNetwork(IPAddress.Parse("10.0.0.0"), 8),
                             new IPNetwork(IPAddress.Parse("172.16.0.0"), 12),
                             new IPNetwork(IPAddress.Parse("192.168.0.0"), 16),
                             new IPNetwork(IPAddress.Parse("127.0.0.0"), 8)
                         })
                {
                    options.KnownIPNetworks.Add(network);
                }
            });
        }

        public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RateLimitingOptions>(configuration.GetSection(RateLimitingOptions.SectionName));

            var limits = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>()
                         ?? new RateLimitingOptions();

            return services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    IsExempt(context)
                        ? RateLimitPartition.GetNoLimiter("exempt")
                        : RateLimitPartition.GetSlidingWindowLimiter(GetPartitionKey(context),
                            _ => new SlidingWindowRateLimiterOptions
                            {
                                PermitLimit = limits.PermitLimit,
                                Window = TimeSpan.FromSeconds(limits.WindowSeconds),
                                SegmentsPerWindow = limits.SegmentsPerWindow,
                                // No queue: a throttled caller is told to retry rather than parked on a
                                // held connection, which would tie up server resources under the load
                                // the limiter exists to shed.
                                QueueLimit = 0
                            }));

                options.AddPolicy(UncachedPolicy, context =>
                    RateLimitPartition.GetSlidingWindowLimiter($"uncached:{GetPartitionKey(context)}",
                        _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = limits.UncachedPermitLimit,
                            Window = TimeSpan.FromSeconds(limits.UncachedWindowSeconds),
                            SegmentsPerWindow = limits.SegmentsPerWindow,
                            QueueLimit = 0
                        }));

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    // Only a fixed or sliding window can say when a permit frees up; a concurrency
                    // limiter cannot, so the header is written only when the metadata is present.
                    var retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                        ? (int)Math.Ceiling(retryAfter.TotalSeconds)
                        : limits.WindowSeconds;

                    context.HttpContext.Response.Headers.RetryAfter =
                        retryAfterSeconds.ToString(NumberFormatInfo.InvariantInfo);

                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("weather_backend.RateLimiting");

                    // The partition key is the caller's address, so it is logged as a parameter rather
                    // than interpolated into the template - same reasoning as LogMiddleware.
                    logger.LogWarning("Rate limit rejected {Method} {Path} for {Partition}",
                        context.HttpContext.Request.Method, context.HttpContext.Request.Path,
                        GetPartitionKey(context.HttpContext));

                    await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Too many requests.",
                        Detail = $"Rate limit exceeded. Retry after {retryAfterSeconds} seconds.",
                        Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}"
                    }, cancellationToken);
                };
            });
        }

        private static bool IsExempt(HttpContext context)
        {
            return ExemptPaths.Any(path =>
                context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// One bucket per caller address. Requests with no remote address - in-memory test hosts, and
        /// anything not arriving over a socket - share a single named partition rather than falling
        /// through unlimited.
        /// </summary>
        private static string GetPartitionKey(HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
