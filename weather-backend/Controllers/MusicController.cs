using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using weather_backend.Repository;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("api/music")]
    [ProducesResponseType(typeof(ProblemDetails), 429)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public class MusicController : ControllerBase
    {
        private readonly IDynamoDbClient _client;
        private readonly ILogger<MusicController> _logger;

        public MusicController(IDynamoDbClient client, ILogger<MusicController> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                _logger.LogInformation(e, "Song title load was cancelled before it completed");
            }

            return BadRequest(new ProblemDetails { Type = "typeisfailed", Detail = "Failed" });
        }
    }
}
