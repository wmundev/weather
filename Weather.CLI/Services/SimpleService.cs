using Microsoft.Extensions.Logging;

namespace Weather.CLI.Services
{
    public sealed class SimpleService(ILogger<SimpleService> logger)
    {
        private readonly ILogger<SimpleService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<int> DoThings()
        {
            Func<Task<int>> func = async () =>
            {
                await Task.Delay(3000);
                await Task.CompletedTask;
                return 1;
            };

            Func<Task<char>> func2 = async () =>
            {
                await Task.Delay(2000);
                await Task.CompletedTask;
                return 'c';
            };

            var result1 = await SafeExecutor(func);
            var result2 = await SafeExecutor(func2);

            return result1 + (int)result2;
        }


        public async Task<T> SafeExecutor<T>(Func<Task<T>> func)
        {
            try
            {
                // Awaited, not just returned: returning the task unawaited puts the delegate's failure
                // outside the try block, so the catch below could never observe it.
                return await func();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while executing the function.");
                throw;
            }
        }
    }
}
