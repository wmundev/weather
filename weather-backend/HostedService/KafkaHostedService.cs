using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace weather_backend.HostedService
{
    public class KafkaHostedService : BackgroundService
    {
        private readonly IConfiguration _configuration;

        public KafkaHostedService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            await BackgroundJob(stoppingToken);
        }

#pragma warning disable CS1998
        private async Task BackgroundJob(CancellationToken stoppingToken)
#pragma warning restore CS1998
        {
            var server = _configuration.GetValue<string>("Kafka:ServerAddress");

            var config = new ConsumerConfig { BootstrapServers = server, GroupId = "foo", AutoOffsetReset = AutoOffsetReset.Earliest };

            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                consumer.Subscribe("weblog");
                var cancelled = false;
                while (!cancelled)
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    // Console.WriteLine("nice" + consumeResult.Message.Key);
                    Console.WriteLine($"Message received: {consumeResult.Message.Value}");
                }

                consumer.Close();
            }

            Console.WriteLine("out");
        }
    }
}
