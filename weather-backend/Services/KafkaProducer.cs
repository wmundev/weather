using System.Net;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace weather_backend.Services
{
    public interface IKafkaProducer
    {
        public Task ProduceMessage(string topic, string message);
    }

    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<Null, string> _producerBuilder;

        public KafkaProducer()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "18.233.153.1:9093",
                ClientId = Dns.GetHostName()
            };
            _producerBuilder = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task ProduceMessage(string topic, string message)
        {
            await _producerBuilder.ProduceAsync(topic, new Message<Null, string> {Value = message});
        }
    }
}