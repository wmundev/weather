using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        /// <summary>
        /// Retrieves the song title from the music data asynchronously, with support for cancellation.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{ActionResult{String}}"/> representing the asynchronous operation,
        /// containing the song title as a string if successful, or a problem detail if the task is canceled.
        /// </returns>
        [HttpGet]
        [Route("song-title")]
        public async Task<ActionResult<string>> GetAsyncCancel()
        {
            var source = new CancellationTokenSource();
            var someTask = _client.LoadMusicDto(source.Token);
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
