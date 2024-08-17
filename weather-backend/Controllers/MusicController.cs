using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using weather_backend.Dto;
using weather_backend.Repository;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("api/music")]
    public class MusicController : ControllerBase
    {
        private readonly IDynamoDbClient _client;

        public MusicController(IDynamoDbClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        [HttpGet]
        [Route("song-title")]
        public async Task<ActionResult<string>> GetAsyncCancel()
        {
            var source = new CancellationTokenSource();
            var someTask = _client.getthings(source.Token);
            await source.CancelAsync();

            try
            {
                var things = await someTask;
                return Ok(things.SongTitle);
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine(e.Message);
            }

            return BadRequest(new ProblemDetails {Type = "typeisfailed", Detail = "Failed"});
        }
    }
}
