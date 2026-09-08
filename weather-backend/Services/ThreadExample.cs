using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace weather_backend.Services
{
    public class ThreadExample
    {
        private readonly ILogger<ThreadExample> _logger;
        private bool isCompleted;

        public ThreadExample(ILogger<ThreadExample> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void CreateNewThread()
        {
            var newThread = new Thread(ForLoopThread);
            newThread.Start();
            newThread.Name = "new thread";

            Thread.CurrentThread.Name = "main thread";
            ForLoopThread();
        }

        private void ForLoopThread()
        {
            if (!isCompleted)
            {
                // Thread.CurrentThread.Name is the point of the example, so it is recorded as a field
                // rather than dropped into the message.
                _logger.LogDebug("Thread example ran on {ThreadName}", Thread.CurrentThread.Name);
                isCompleted = true;
            }
        }
    }
}
