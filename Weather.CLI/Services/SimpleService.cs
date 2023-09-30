namespace Weather.CLI.Services
{
    public class SimpleService
    {
        public async Task DoThings()
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

            await SafeExecutor<int>(func);
        }


        private Task<T> SafeExecutor<T>(Func<Task<T>> func)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            throw new Exception();
        }
    }
}
