// using System;
// using System.Net;
// using System.Threading.Tasks;
// using Confluent.Kafka;
// using Microsoft.Extensions.Configuration;
//
// namespace weather_backend.Services
// {
//     public interface IKafkaProducer
//     {
//         public Task ProduceMessage(string topic, string message);
//     }
//
//     public class KafkaProducer : IKafkaProducer
//     {
//         private readonly IConfiguration _configuration;
//         private readonly IProducer<Null, string> _producerBuilder;
//
//         public KafkaProducer(IConfiguration configuration)
//         {
//             _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
//             var server = _configuration.GetValue<string>("Kafka:ServerAddress");
//             var config = new ProducerConfig
//             {
//                 BootstrapServers = server,
//                 ClientId = Dns.GetHostName(),
//                 MessageSendMaxRetries = 10
//             };
//             _producerBuilder = new ProducerBuilder<Null, string>(config).Build();
//         }
//
//         public async Task ProduceMessage(string topic, string message)
//         {
//             await _producerBuilder.ProduceAsync(topic, new Message<Null, string> {Value = message});
//         }
//     }
// }


