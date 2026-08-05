using System;
using System.Threading;
using System.Threading.Tasks;
using weather_backend.Services;
using weather_test.TestHelpers;
using Xunit;

namespace weather_test.Services
{
    public sealed class CronJobServiceTest
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Runs on the minute, so it never actually fires during a test - what is under test here is the
        /// scheduling and shutdown path, which used to throw and used to leave a timer running.
        /// </summary>
        private sealed class TestCronJob : CronJobService
        {
            public TestCronJob(string cronExpression, RecordingLogger<TestCronJob> logger)
                : base(cronExpression, TimeZoneInfo.Utc, logger)
            {
            }

            public int WorkCount { get; private set; }

            public override Task DoWork(CancellationToken cancellationToken)
            {
                WorkCount++;
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task StartAsync_SchedulesWithoutThrowing()
        {
            var job = new TestCronJob("* * * * *", new RecordingLogger<TestCronJob>());

            // Scheduling used to fall through its own guard into a timer with a non-positive interval.
            await job.StartAsync(CancellationToken.None);

            await job.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task StartAsync_DoesNotBlockTheHost()
        {
            var job = new TestCronJob("0 22 * * *", new RecordingLogger<TestCronJob>());

            var start = job.StartAsync(CancellationToken.None);

            Assert.Same(start, await Task.WhenAny(start, Task.Delay(Timeout)));
            await job.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task StopAsync_StopsTheJobPromptly()
        {
            var job = new TestCronJob("0 22 * * *", new RecordingLogger<TestCronJob>());
            await job.StartAsync(CancellationToken.None);

            var stop = job.StopAsync(CancellationToken.None);

            // The Scheduler subclass used to override StopAsync and discard the base implementation, so
            // the job stayed scheduled after the host had shut down.
            Assert.Same(stop, await Task.WhenAny(stop, Task.Delay(Timeout)));
            Assert.Equal(0, job.WorkCount);
        }

        [Fact]
        public async Task ExecuteTask_CompletesWhenTheJobIsStopped()
        {
            var job = new TestCronJob("0 22 * * *", new RecordingLogger<TestCronJob>());
            await job.StartAsync(CancellationToken.None);

            await job.StopAsync(CancellationToken.None);

            Assert.NotNull(job.ExecuteTask);
            Assert.True(job.ExecuteTask!.IsCompleted);
        }
    }
}
