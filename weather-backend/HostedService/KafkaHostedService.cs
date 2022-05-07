using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;

namespace weather_backend.HostedService
{
    public class KafkaHostedService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            await BackgroundJob(stoppingToken);
        }

        private async Task BackgroundJob(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "18.233.153.1:9093",
                GroupId = "foo",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                consumer.Subscribe("weblog");
                var cancelled = false;
                while (!cancelled)
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    Console.WriteLine("nice" + consumeResult.Message.Key);
                    Console.WriteLine("nice" + consumeResult.Message.Value);
                }

                consumer.Close();
            }

            Console.WriteLine("out");
        }
    }
}