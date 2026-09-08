using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace weather_backend.Middleware
{
    public class LogMiddleware
    {
        public const string CorrelationIdHeader = "CorrelationID";

        /// <summary>
        /// Upper bound on a caller-supplied correlation id. The value is echoed in a response header and
        /// attached to every record for the request, so an unbounded one would bloat both.
        /// </summary>
        private const int MaxCorrelationIdLength = 64;

        private readonly ILogger<LogMiddleware> _logger;
        private readonly RequestDelegate _next;

        public LogMiddleware(RequestDelegate next, ILogger<LogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = ResolveCorrelationId(context);

            // Set on the way out rather than now: a later middleware may replace the response, and
            // writing a header after the response has started throws.
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeader] = correlationId;
                return Task.CompletedTask;
            });

            // A scope, not a single log line: every record written while handling this request - by our
            // code and by the framework - carries these fields, so one request can be reassembled from
            // the log store without correlating on timestamps.
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["RequestMethod"] = context.Request.Method,
                // Path only. The query string is caller-controlled and is where identifiers and
                // any future token would sit, so it is deliberately not logged.
                ["RequestPath"] = context.Request.Path.Value ?? string.Empty
            }))
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    await _next(context);
                }
                finally
                {
                    stopwatch.Stop();

                    // In the finally block so a request that fails is still recorded with its duration;
                    // GlobalExceptionHandler will have already logged the exception itself.
                    _logger.LogInformation(
                        "Handled {RequestMethod} {RequestPath} responded {StatusCode} in {ElapsedMilliseconds}ms",
                        context.Request.Method, context.Request.Path.Value, context.Response.StatusCode,
                        stopwatch.ElapsedMilliseconds);
                }
            }
        }

        /// <summary>
        /// Takes the caller's correlation id when it is well formed, and mints one otherwise.
        /// </summary>
        /// <remarks>
        /// The header is caller-controlled and is echoed back in a response header, so it is validated
        /// rather than trusted: anything over-long or carrying characters outside the safe set - control
        /// characters and newlines among them - is replaced instead of being passed through.
        /// </remarks>
        private static string ResolveCorrelationId(HttpContext context)
        {
            var supplied = context.Request.Headers[CorrelationIdHeader].ToString();

            if (!string.IsNullOrEmpty(supplied) && supplied.Length <= MaxCorrelationIdLength && IsSafe(supplied))
            {
                return supplied;
            }

            // Falls back to the trace identifier so the id still ties to the framework's own scope.
            return string.IsNullOrEmpty(context.TraceIdentifier)
                ? Guid.NewGuid().ToString("N")
                : context.TraceIdentifier;
        }

        private static bool IsSafe(string value)
        {
            foreach (var character in value)
            {
                var isAllowed = char.IsAsciiLetterOrDigit(character)
                                || character is '-' or '_' or '.' or ':';
                if (!isAllowed)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public static class RequestLogMiddleware
    {
        public static IApplicationBuilder UseLogMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogMiddleware>();
        }
    }
}
