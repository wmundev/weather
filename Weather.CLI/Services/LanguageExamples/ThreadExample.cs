using Microsoft.Extensions.Logging;

namespace Weather.CLI.Services.LanguageExamples
{
    /// <summary>
    /// Runs the same method on a new thread and on the calling thread. Moved from the API, which registered it
    /// but never used it.
    /// </summary>
    public sealed class ThreadExample(ILogger<ThreadExample> logger)
    {
        private readonly ILogger<ThreadExample> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Runs the work on a named thread, waits for it, then runs it again on the calling thread. The join keeps
        /// the two runs from logging at the same time.
        /// </summary>
        public void RunOnTwoThreads()
        {
            var newThread = new Thread(LogCurrentThread) {Name = "new thread"};
            newThread.Start();
            newThread.Join();

            LogCurrentThread();
        }

        private void LogCurrentThread()
        {
            _logger.LogInformation("Thread example ran on {ThreadName}", Thread.CurrentThread.Name ?? "unnamed");
        }
    }
}
