using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using weather_backend.HostedService;
using weather_backend.Services;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KafkaController : ControllerBase
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IKafkaProducer _kafkaProducer;

        public KafkaController(IKafkaProducer kafkaProducer, IBackgroundTaskQueue backgroundTaskQueue)
        {
            _kafkaProducer = kafkaProducer ?? throw new ArgumentNullException(nameof(kafkaProducer));
            _backgroundTaskQueue = backgroundTaskQueue ?? throw new ArgumentNullException(nameof(backgroundTaskQueue));
        }

        [HttpGet]
        [Route("/niceone")]
        public async Task<ActionResult> GetKafka([FromQuery] string message, [FromQuery] string topic)
        {
            // Task.Factory.StartNew(() => _kafkaProducer.ProduceMessage(topic, message));
            await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(async token =>
            {
                // await BuildWorkItemAsync(token, message, topic);
                Console.WriteLine("haha");
                Thread.Sleep(1000);
            });

            return Ok();
        }

        private async ValueTask BuildWorkItemAsync(CancellationToken token, string message, string topic)
        {
            await _kafkaProducer.ProduceMessage(topic, message);
        }
    }
}