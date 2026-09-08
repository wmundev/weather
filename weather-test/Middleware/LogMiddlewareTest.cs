using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using weather_backend.Middleware;
using weather_test.TestHelpers;
using Xunit;

namespace weather_test.Middleware
{
    public class LogMiddlewareTest
    {
        private readonly RecordingLogger<LogMiddleware> _logger = new();

        private static DefaultHttpContext CreateContext(string? correlationId = null)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/weather/city";
            context.TraceIdentifier = "trace-abc";

            if (correlationId is not null)
            {
                context.Request.Headers[LogMiddleware.CorrelationIdHeader] = correlationId;
            }

            return context;
        }

        private LogMiddleware CreateMiddleware(RequestDelegate next)
        {
            return new LogMiddleware(next, _logger);
        }

        private static object? ValueOf(IReadOnlyList<KeyValuePair<string, object?>> entry, string key)
        {
            return entry.FirstOrDefault(pair => pair.Key == key).Value;
        }

        private IReadOnlyList<KeyValuePair<string, object?>> SingleEntry()
        {
            return Assert.Single(_logger.Entries);
        }

        /// <summary>
        /// Scope state only has to be an enumerable of key/value pairs - that is the shape the console
        /// formatters unpack into fields - so the assertion is made against that rather than a concrete
        /// collection type.
        /// </summary>
        private IReadOnlyList<KeyValuePair<string, object?>> SingleScope()
        {
            var scope = Assert.Single(_logger.Scopes);
            var pairs = Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object>>>(scope);
            return pairs.Select(pair => new KeyValuePair<string, object?>(pair.Key, pair.Value)).ToList();
        }

        [Fact]
        public async Task InvokeAsync_WhenRequestSucceeds_ShouldLogMethodPathStatusAndDurationAsNamedFields()
        {
            var context = CreateContext();
            var middleware = CreateMiddleware(ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status200OK;
                return Task.CompletedTask;
            });

            await middleware.InvokeAsync(context);

            var entry = SingleEntry();
            Assert.Equal("GET", ValueOf(entry, "RequestMethod"));
            Assert.Equal("/weather/city", ValueOf(entry, "RequestPath"));
            Assert.Equal(200, ValueOf(entry, "StatusCode"));
            Assert.NotNull(ValueOf(entry, "ElapsedMilliseconds"));
        }

        [Fact]
        public async Task InvokeAsync_WhenCorrelationIdSupplied_ShouldPutItInTheScope()
        {
            var context = CreateContext("abc-123");
            var middleware = CreateMiddleware(_ => Task.CompletedTask);

            await middleware.InvokeAsync(context);

            Assert.Equal("abc-123", ValueOf(SingleScope(), "CorrelationId"));
        }

        [Fact]
        public async Task InvokeAsync_WhenCorrelationIdMissing_ShouldFallBackToTheTraceIdentifier()
        {
            var context = CreateContext();
            var middleware = CreateMiddleware(_ => Task.CompletedTask);

            await middleware.InvokeAsync(context);

            Assert.Equal("trace-abc", ValueOf(SingleScope(), "CorrelationId"));
        }

        [Theory]
        // Newlines are the log-forging vector, and the value is echoed in a response header where a
        // carriage return would split it. Over-length values are rejected on the same principle.
        [InlineData("abc\r\nInjected: value")]
        [InlineData("has spaces")]
        [InlineData("{Braces}")]
        public async Task InvokeAsync_WhenCorrelationIdIsMalformed_ShouldReplaceItRatherThanEchoIt(string supplied)
        {
            var context = CreateContext(supplied);
            var middleware = CreateMiddleware(_ => Task.CompletedTask);

            await middleware.InvokeAsync(context);

            Assert.Equal("trace-abc", ValueOf(SingleScope(), "CorrelationId"));
        }

        [Fact]
        public async Task InvokeAsync_WhenCorrelationIdIsTooLong_ShouldReplaceIt()
        {
            var context = CreateContext(new string('a', 65));
            var middleware = CreateMiddleware(_ => Task.CompletedTask);

            await middleware.InvokeAsync(context);

            Assert.Equal("trace-abc", ValueOf(SingleScope(), "CorrelationId"));
        }

        [Fact]
        public async Task InvokeAsync_WhenDownstreamThrows_ShouldStillLogTheRequestAndRethrow()
        {
            var context = CreateContext();
            var middleware = CreateMiddleware(_ => throw new InvalidOperationException("boom"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));

            // A request that failed is the one most worth having a record of, so the completion log must
            // come from a finally rather than the success path.
            Assert.Equal("/weather/city", ValueOf(SingleEntry(), "RequestPath"));
        }
    }
}
