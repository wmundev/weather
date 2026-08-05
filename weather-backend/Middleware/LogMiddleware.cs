using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace weather_backend.Middleware
{
    public class LogMiddleware
    {
        private readonly ILogger<LogMiddleware> _logger;
        private readonly RequestDelegate _next;

        public LogMiddleware(RequestDelegate next, ILogger<LogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var headers = context.Request.Headers;
            // headers are case insensitive
            var correlationId = headers["CorrelationID"];
            if (!string.IsNullOrEmpty(correlationId))
                // The header is caller-controlled, so it must be passed as a log parameter and never as
                // part of the message template - otherwise braces in the value corrupt structured
                // formatting and newlines can be used to forge log entries.
                _logger.LogInformation("Correlation: {CorrelationId}", correlationId.ToString());

            await _next(context);
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