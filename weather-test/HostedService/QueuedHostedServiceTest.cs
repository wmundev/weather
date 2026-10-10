using System;
using System.Threading;
using System.Threading.Tasks;
using weather_backend.HostedService;
using weather_test.TestHelpers;
using Xunit;

namespace weather_test.HostedService
{
    public sealed class QueuedHostedServiceTest
    {
        [Fact]
        public async Task StopAsync_WhenItemsAreQueued_ShouldRunThemAndStopWithoutLoggingAnError()
        {
            var queue = new DefaultBackgroundTaskQueue(10);
            var logger = new RecordingLogger<QueuedHostedService>();
            var sut = new QueuedHostedService(queue, logger);
            var completed = 0;

            await sut.StartAsync(CancellationToken.None);
            for (var i = 0; i < 3; i++)
            {
                await queue.QueueBackgroundWorkItemAsync(_ =>
                {
                    Interlocked.Increment(ref completed);
                    return ValueTask.CompletedTask;
                });
            }

            await sut.StopAsync(CancellationToken.None);

            Assert.Equal(3, completed);
            // Draining the queue closes the channel; reading a closed channel used to be logged as a failed
            // work item, over and over, until the host cancelled the loop.
            Assert.DoesNotContain(logger.Messages, message => message.Contains("Error occurred", StringComparison.Ordinal));
        }

        [Fact]
        public async Task StopAsync_WhenCalledTwice_ShouldNotThrow()
        {
            var sut = new QueuedHostedService(new DefaultBackgroundTaskQueue(10), new RecordingLogger<QueuedHostedService>());
            await sut.StartAsync(CancellationToken.None);

            await sut.StopAsync(CancellationToken.None);
            var exception = await Record.ExceptionAsync(() => sut.StopAsync(CancellationToken.None));

            Assert.Null(exception);
        }

        [Fact]
        public async Task QueueBackgroundWorkItemAsync_WhenManyThreadsWrite_ShouldDeliverEveryItem()
        {
            var queue = new DefaultBackgroundTaskQueue(1000);

            await Task.WhenAll(Repeat(200, _ => Task.Run(async () =>
                await queue.QueueBackgroundWorkItemAsync(_ => ValueTask.CompletedTask))));

            for (var i = 0; i < 200; i++)
            {
                await queue.DequeueAsync(CancellationToken.None);
            }
        }

        private static Task[] Repeat(int count, Func<int, Task> factory)
        {
            var tasks = new Task[count];
            for (var i = 0; i < count; i++)
            {
                tasks[i] = factory(i);
            }

            return tasks;
        }
    }
}
