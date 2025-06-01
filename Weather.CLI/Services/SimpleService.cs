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


        public Task<T> SafeExecutor<T>(Func<Task<T>> func)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while executing the function.");
            }

            throw new Exception();
        }
    }
}
