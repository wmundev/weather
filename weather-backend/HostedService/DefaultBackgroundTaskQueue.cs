using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace weather_backend.HostedService
{
    public class DefaultBackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<Func<CancellationToken, ValueTask>> _queue;

        public DefaultBackgroundTaskQueue(int capacity)
        {
            BoundedChannelOptions options = new(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = true
            };
            _queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
        }

        public async ValueTask QueueBackgroundWorkItemAsync(
            Func<CancellationToken, ValueTask> workItem)
        {
            if (workItem is null) throw new ArgumentNullException(nameof(workItem));

            await _queue.Writer.WriteAsync(workItem);
        }

        public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
            CancellationToken cancellationToken)
        {
            var workItem =
                await _queue.Reader.ReadAsync(cancellationToken);

            return workItem;
        }

        public async Task CompletionAsync()
        {
            _queue.Writer.Complete();
            await _queue.Reader.Completion;
        }
    }
}