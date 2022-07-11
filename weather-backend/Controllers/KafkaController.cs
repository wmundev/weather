using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using weather_backend.Services;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KafkaController : ControllerBase
    {
        private readonly IKafkaProducer _kafkaProducer;

        public KafkaController(IKafkaProducer kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;
        }

        [HttpGet]
        [Route("/niceone")]
        public async Task<ActionResult> GetKafka([FromQuery] string message, [FromQuery] string topic)
        {
            Task.Factory.StartNew(() => _kafkaProducer.ProduceMessage(topic, message));

            return Ok();
        }
    }
}