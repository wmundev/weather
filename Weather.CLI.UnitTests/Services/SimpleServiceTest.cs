using Microsoft.Extensions.Logging;
using NSubstitute;
using Weather.CLI.Services;

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
    }
}
