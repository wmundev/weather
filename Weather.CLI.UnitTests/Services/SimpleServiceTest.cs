using Microsoft.Extensions.Logging;
using NSubstitute;
using Weather.CLI.Services;
using Weather.CLI.UnitTests.TestHelpers;

namespace Weather.CLI.UnitTests.Services
{
    public class SimpleServiceTest
    {
        [Fact]
        public async Task DoThings_WhenCalled_Returns100()
        {
            // Arrange
            var logger = Substitute.For<ILogger<SimpleService>>();
            var simpleService = new SimpleService(logger);

            // Act
            var result = await simpleService.DoThings();

            // Assert

            Assert.Equal(100, result);
        }

        [Fact]
        public async Task SafeExecutor_WhenCalledWithFuncThatThrowsException_ThrowsException()
        {
            // Arrange
            var logger = Substitute.For<ILogger<SimpleService>>();
            var simpleService = new SimpleService(logger);

            // Act
            Func<Task<int>> func = async () =>
            {
                await Task.Delay(3000);
                throw new Exception();
            };

            // Assert
            await Assert.ThrowsAsync<Exception>(() => simpleService.SafeExecutor<int>(func));
        }

        [Fact]
        public async Task SafeExecutor_WhenTheDelegateFails_LogsTheFailure()
        {
            // Arrange
            var logger = new RecordingLogger<SimpleService>();
            var simpleService = new SimpleService(logger);

            Func<Task<int>> func = async () =>
            {
                await Task.Yield();
                throw new InvalidOperationException("boom");
            };

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(() => simpleService.SafeExecutor(func));

            // Assert: returning the task without awaiting it put the failure outside the try block, so
            // the catch - and this log call - could never run.
            Assert.Contains(logger.Messages, message => message.Contains("An error occurred", StringComparison.Ordinal));
        }
    }
}
