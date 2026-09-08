using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace weather_backend.Middleware
{
    /// <summary>
    /// Last-resort handler for exceptions no controller caught. Controllers that map their own
    /// failures - such as <c>WeatherForecastController.GetOrFetchAsync</c>, which turns
    /// <see cref="System.Net.Http.HttpRequestException"/> into a 404 - still win, because this only
    /// sees what reaches the top of the pipeline.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// Non-standard status used by nginx and others for "client closed the request". Nothing is
        /// written to a connection the caller has already dropped; it exists to keep the 5xx metrics
        /// honest, since an abandoned request is not a server fault.
        /// </summary>
        private const int StatusClientClosedRequest = 499;

        private readonly IHostEnvironment _environment;
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger,
            IProblemDetailsService problemDetailsService, IHostEnvironment environment)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _problemDetailsService = problemDetailsService ?? throw new ArgumentNullException(nameof(problemDetailsService));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
            CancellationToken cancellationToken)
        {
            var method = httpContext.Request.Method;
            var path = httpContext.Request.Path;

            if (httpContext.Response.HasStarted)
            {
                // The status line and headers are already on the wire, so there is no response left to
                // replace with a ProblemDetails body. Returning false lets the server abort the
                // connection rather than throwing a second exception on top of the first.
                _logger.LogError(exception,
                    "Unhandled exception after the response had started for {Method} {Path}", method, path);
                return false;
            }

            if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation("Client cancelled {Method} {Path}", method, path);
                httpContext.Response.StatusCode = StatusClientClosedRequest;
                return true;
            }

            _logger.LogError(exception, "Unhandled exception for {Method} {Path}", method, path);

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1",
                Instance = $"{method} {path}"
            };

            // Correlates the opaque response with the log entry that has the stack trace. The
            // CorrelationID header is the one LogMiddleware already logs, so a caller supplying it can
            // quote it back; TraceIdentifier covers callers that do not.
            problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            var correlationId = httpContext.Request.Headers["CorrelationID"].ToString();
            if (!string.IsNullOrEmpty(correlationId))
            {
                problemDetails.Extensions["correlationId"] = correlationId;
            }

            // Exception text routinely carries connection strings, query strings and API keys - the
            // OpenWeatherMap key travels in a query parameter - so it is only ever returned locally.
            if (_environment.IsDevelopment())
            {
                problemDetails.Detail = exception.ToString();
            }

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            });
        }
    }
}
