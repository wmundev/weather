﻿using System.Text;
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
            {
                var strings = new StringBuilder();
                strings.Append("Correlation: ");
                strings.Append(correlationId);
                _logger.LogInformation(strings.ToString());
            }

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