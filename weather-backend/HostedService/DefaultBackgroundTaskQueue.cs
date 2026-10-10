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
                // One QueuedHostedService reads; any number of request threads may write. SingleWriter = true
                // promises the channel a single writer, and breaking that promise is undefined behaviour.
                SingleReader = true,
                SingleWriter = false
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
            // TryComplete, not Complete: a host may stop its hosted services more than once (the test host
            // does when it is disposed), and Complete throws on a channel that is already closed.
            _queue.Writer.TryComplete();
            await _queue.Reader.Completion;
        }
    }
}