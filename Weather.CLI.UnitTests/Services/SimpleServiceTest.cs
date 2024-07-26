using Weather.CLI.Services;

namespace Weather.CLI.UnitTests.Services
{
    public class SimpleServiceTest
    {
        [Fact]
        public async Task DoThings_WhenCalled_Returns100()
        {
            // Arrange
            var simpleService = new SimpleService();

            // Act
            var result = await simpleService.DoThings();

            // Assert

            Assert.Equal(100, result);
        }

        [Fact]
        public async Task SafeExecutor_WhenCalledWithFuncThatThrowsException_ThrowsException()
        {
            // Arrange
            var simpleService = new SimpleService();

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
