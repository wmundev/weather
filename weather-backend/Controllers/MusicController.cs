using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using weather_backend.Dto;
using weather_backend.Repository;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MusicController : ControllerBase
    {
        private readonly IDynamoDbClient _client;

        public MusicController(IDynamoDbClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        [HttpGet]
        [Route("music")]
        public async Task<EmailCodeEntity> GetIpAddress()
        {
            // return (await _client.getthings()).SongTitle;
            return await _client.LoadEmailCode();
        }

        [HttpGet]
        [Route("music/nice")]
        public async Task<ActionResult<string>> GetAsyncCancel()
        {
            var source = new CancellationTokenSource();
            var someTask = _client.getthings(source.Token);
            source.Cancel();

            try
            {
                var things = await someTask;
                return Ok(things.SongTitle);
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine(e.Message);
            }

            return BadRequest(new ProblemDetails { Type = "typeisfailed", Detail = "Failed" });
        }
    }
}
