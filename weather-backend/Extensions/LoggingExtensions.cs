using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace weather_backend.Extensions
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// Configures the log providers: human-readable locally, one JSON object per line everywhere
        /// else.
        /// </summary>
        /// <remarks>
        /// Lives here rather than inline in <c>Program.CreateHostBuilder</c> so the output format can be
        /// asserted on directly - the shape of a production log record is the thing that breaks
        /// silently, and it is not otherwise reachable from a test.
        /// </remarks>
        public static ILoggingBuilder AddStructuredLogging(this ILoggingBuilder logging, IHostEnvironment environment)
        {
            // CreateDefaultBuilder has already registered Console, Debug and EventSource. Clearing first
            // avoids every record being written twice once a second console provider is added below.
            logging.ClearProviders();

            if (environment.IsDevelopment())
            {
                logging.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                });
            }
            else
            {
                // What the container log driver and New Relic can actually parse: the message template,
                // its named parameters and the active scopes each land in their own field rather than
                // being flattened into a single rendered string.
                logging.AddJsonConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.UseUtcTimestamp = true;
                    options.TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
                });
            }

            // Puts TraceId and SpanId on every scope, so records emitted by framework code that knows
            // nothing about our correlation id can still be tied back to one request.
            logging.Configure(options =>
            {
                options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId
                                                  | ActivityTrackingOptions.SpanId
                                                  | ActivityTrackingOptions.ParentId;
            });

            return logging;
        }
    }
}
