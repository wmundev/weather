using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using weather_backend.Middleware;
using Xunit;

namespace weather_test.Middleware
{
    public class GlobalExceptionHandlerTest
    {
        private readonly ILogger<GlobalExceptionHandler> _mockLogger;
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandlerTest()
        {
            _mockLogger = Substitute.For<ILogger<GlobalExceptionHandler>>();

            // The real writer, not a substitute: these tests assert on the JSON that reaches the caller,
            // and a substitute would report success without writing a body to inspect.
            _problemDetailsService = new ServiceCollection()
                .AddProblemDetails()
                .AddLogging()
                .BuildServiceProvider()
                .GetRequiredService<IProblemDetailsService>();
        }

        private GlobalExceptionHandler CreateHandler(string environmentName)
        {
            var environment = Substitute.For<IHostEnvironment>();
            environment.EnvironmentName.Returns(environmentName);
            return new GlobalExceptionHandler(_mockLogger, _problemDetailsService, environment);
        }

        private static DefaultHttpContext CreateContext()
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/weather/city";
            context.Request.Headers["Accept"] = "application/json";
            context.Response.Body = new MemoryStream();
            return context;
        }

        private static async Task<JsonElement> ReadBodyAsync(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var document = await JsonDocument.ParseAsync(context.Response.Body);
            return document.RootElement.Clone();
        }

        [Fact]
        public async Task TryHandleAsync_WhenExceptionIsUnhandled_ShouldWriteProblemDetailsWith500()
        {
            var handler = CreateHandler(Environments.Production);
            var context = CreateContext();

            var handled = await handler.TryHandleAsync(context, new InvalidOperationException("boom"), CancellationToken.None);

            Assert.True(handled);
            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

            var body = await ReadBodyAsync(context);
            Assert.Equal("An unexpected error occurred.", body.GetProperty("title").GetString());
            Assert.Equal(500, body.GetProperty("status").GetInt32());
            Assert.Equal("GET /weather/city", body.GetProperty("instance").GetString());
        }

        [Fact]
        public async Task TryHandleAsync_WhenNotDevelopment_ShouldNotLeakExceptionDetail()
        {
            var handler = CreateHandler(Environments.Production);
            var context = CreateContext();

            await handler.TryHandleAsync(context, new InvalidOperationException("appid=super-secret-key"), CancellationToken.None);

            var body = await ReadBodyAsync(context);
            Assert.False(body.TryGetProperty("detail", out _));
            Assert.DoesNotContain("super-secret-key", body.ToString());
        }

        [Fact]
        public async Task TryHandleAsync_WhenDevelopment_ShouldIncludeExceptionDetail()
        {
            var handler = CreateHandler(Environments.Development);
            var context = CreateContext();

            await handler.TryHandleAsync(context, new InvalidOperationException("boom"), CancellationToken.None);

            var body = await ReadBodyAsync(context);
            Assert.Contains("boom", body.GetProperty("detail").GetString());
        }

        [Fact]
        public async Task TryHandleAsync_WhenCorrelationIdHeaderIsPresent_ShouldEchoItBack()
        {
            var handler = CreateHandler(Environments.Production);
            var context = CreateContext();
            context.Request.Headers["CorrelationID"] = "abc-123";

            await handler.TryHandleAsync(context, new InvalidOperationException("boom"), CancellationToken.None);

            var body = await ReadBodyAsync(context);
            Assert.Equal("abc-123", body.GetProperty("correlationId").GetString());
            Assert.False(string.IsNullOrEmpty(body.GetProperty("traceId").GetString()));
        }

        [Fact]
        public async Task TryHandleAsync_WhenClientAbortedTheRequest_ShouldReturn499AndNotWriteABody()
        {
            var handler = CreateHandler(Environments.Production);
            var context = CreateContext();
            var aborted = new CancellationTokenSource();
            await aborted.CancelAsync();
            context.RequestAborted = aborted.Token;

            var handled = await handler.TryHandleAsync(context, new OperationCanceledException(), CancellationToken.None);

            Assert.True(handled);
            Assert.Equal(499, context.Response.StatusCode);
            Assert.Equal(0, context.Response.Body.Length);
        }

        [Fact]
        public async Task TryHandleAsync_WhenResponseHasAlreadyStarted_ShouldDeclineToHandle()
        {
            var handler = CreateHandler(Environments.Production);
            var context = CreateContext();

            // Once headers are on the wire the status code cannot be replaced, so the handler must bow
            // out and let the server tear the connection down.
            var responseFeature = Substitute.For<Microsoft.AspNetCore.Http.Features.IHttpResponseFeature>();
            responseFeature.HasStarted.Returns(true);
            responseFeature.Headers.Returns(new HeaderDictionary());
            context.Features.Set(responseFeature);

            var handled = await handler.TryHandleAsync(context, new InvalidOperationException("boom"), CancellationToken.None);

            Assert.False(handled);
        }
    }
}
