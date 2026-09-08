using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using weather_backend.Extensions;
using Xunit;

namespace weather_test.Extensions
{
    public class LoggingExtensionsTest
    {
        /// <summary>
        /// Runs one log record through the real provider configuration and hands back what was written
        /// to stdout. The console providers write on a background thread, so the factory is disposed
        /// before the buffer is read to make sure the queue has drained.
        /// </summary>
        private static string CaptureOutput(string environmentName, Action<ILogger> write)
        {
            var environment = Substitute.For<IHostEnvironment>();
            environment.EnvironmentName.Returns(environmentName);

            var originalOut = Console.Out;
            var buffer = new StringWriter();
            Console.SetOut(buffer);

            try
            {
                var factory = LoggerFactory.Create(logging =>
                {
                    logging.AddStructuredLogging(environment);
                    logging.SetMinimumLevel(LogLevel.Debug);
                });

                write(factory.CreateLogger("weather_test.LoggingProbe"));
                factory.Dispose();
            }
            finally
            {
                Console.SetOut(originalOut);
            }

            return buffer.ToString();
        }

        [Fact]
        public void AddStructuredLogging_OutsideDevelopment_ShouldWriteParsableJsonWithNamedFields()
        {
            var output = CaptureOutput(Environments.Production,
                logger => logger.LogInformation(
                    "Handled {RequestMethod} {RequestPath} responded {StatusCode} in {ElapsedMilliseconds}ms",
                    "GET", "/weather/city", 200, 12));

            Assert.False(string.IsNullOrWhiteSpace(output));

            using var document = JsonDocument.Parse(output);
            var root = document.RootElement;

            // The point of the exercise: the values are queryable fields, not fragments of a rendered
            // sentence that a log store would have to regex out.
            var state = root.GetProperty("State");
            Assert.Equal("GET", state.GetProperty("RequestMethod").GetString());
            Assert.Equal("/weather/city", state.GetProperty("RequestPath").GetString());
            Assert.Equal(200, state.GetProperty("StatusCode").GetInt32());

            // The template itself is preserved, which is what lets a log store group every instance of
            // this record regardless of the values it carried.
            Assert.Equal("Handled {RequestMethod} {RequestPath} responded {StatusCode} in {ElapsedMilliseconds}ms",
                state.GetProperty("{OriginalFormat}").GetString());

            Assert.Equal("Information", root.GetProperty("LogLevel").GetString());
        }

        [Fact]
        public void AddStructuredLogging_OutsideDevelopment_ShouldIncludeScopesAsFields()
        {
            var output = CaptureOutput(Environments.Production, logger =>
            {
                using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = "order-4821" }))
                {
                    logger.LogInformation("Something happened");
                }
            });

            using var document = JsonDocument.Parse(output);

            // Without IncludeScopes the correlation id set by LogMiddleware would never reach the sink,
            // which is the whole reason the scope exists.
            Assert.Contains("order-4821", document.RootElement.GetProperty("Scopes").ToString());
        }

        [Fact]
        public void AddStructuredLogging_InDevelopment_ShouldWritePlainTextRatherThanJson()
        {
            var output = CaptureOutput(Environments.Development,
                logger => logger.LogInformation("Handled {RequestMethod}", "GET"));

            Assert.Contains("Handled GET", output);
            Assert.ThrowsAny<JsonException>(() => JsonDocument.Parse(output));
        }
    }
}
