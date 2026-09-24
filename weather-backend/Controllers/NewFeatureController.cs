using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using weather_backend.Dto;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("feature")]
    [ProducesResponseType(typeof(ProblemDetails), 429)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public class NewFeatureController : ControllerBase
    {
        private readonly ILogger<NewFeatureController> _logger;
        private readonly IConnectionMultiplexer? _redis;

        /// <param name="logger">Logger for this controller.</param>
        /// <param name="redis">
        /// Null when <c>Redis:ConnectionString</c> is not configured. Optional so that the Redis-free routes
        /// here keep working without it; the Redis routes answer 503 instead.
        /// </param>
        public NewFeatureController(ILogger<NewFeatureController> logger, IConnectionMultiplexer? redis = null)
        {
            _redis = redis;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the "pattern" route and returns a simple greeting message.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the action.
        /// </returns>
        [Route("pattern")]
        [HttpGet]
        public IActionResult Pattern()
        {
            var greeting = "nice one";

            if (greeting is { } thisisgreeting) _logger.LogDebug("Pattern matched {Greeting}", thisisgreeting);

            return Ok();
        }

        /// <summary>
        /// TODO: Generates a password and returns it.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the action.
        /// </returns>
        [Route("password")]
        [HttpGet]
        public IActionResult GeneratePassword()
        {
            return Ok();
        }

        /// <summary>
        /// Saves a value to Redis with the key "foo".
        /// </summary>
        /// <param name="value">The value to save in Redis.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the action.
        /// </returns>
        /// <response code="200">Returns the result of the write.</response>
        /// <response code="503">If Redis is not configured.</response>
        [Route("redis")]
        [HttpPost]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 503)]
        public async Task<IActionResult> RedisSaveTest([FromBody] RedisSaveTestDto value)
        {
            if (_redis is null)
            {
                return RedisNotConfigured();
            }

            var db = _redis.GetDatabase();
            var foo = await db.StringSetAsync("foo", value.Value);
            return Ok(foo.ToString());
            // var pong = await db.PingAsync();
        }

        /// <summary>
        /// Retrieves the value associated with the key "foo" from Redis and returns it along with the elapsed time.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the retrieved value and the elapsed time in milliseconds.
        /// </returns>
        /// <response code="200">Returns the stored value and the read latency.</response>
        /// <response code="503">If Redis is not configured.</response>
        [Route("redis")]
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ProblemDetails), 503)]
        public async Task<IActionResult> RedisGetTest()
        {
            if (_redis is null)
            {
                return RedisNotConfigured();
            }

            var watch = Stopwatch.StartNew();

            var db = _redis.GetDatabase();
            var foo = await db.StringGetAsync("foo");

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            return Ok(new { result = foo.ToString(), time = elapsedMs });
            // var pong = await db.PingAsync();
        }

        private ObjectResult RedisNotConfigured()
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new ProblemDetails
                {
                    Status = StatusCodes.Status503ServiceUnavailable,
                    Title = "Redis is not configured.",
                    Detail = "Set Redis:ConnectionString to enable the Redis endpoints."
                });
        }
    }
}
