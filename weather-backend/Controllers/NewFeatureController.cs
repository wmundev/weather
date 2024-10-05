using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using weather_backend.Dto;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("feature")]
    public class NewFeatureController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public NewFeatureController(IConnectionMultiplexer redis)
        {
            _redis = redis;
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

            if (greeting is { } thisisgreeting) Console.WriteLine(thisisgreeting);

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
        [Route("redis")]
        [HttpPost]
        public async Task<IActionResult> RedisSaveTest([FromBody] RedisSaveTestDto value)
        {
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
        [Route("redis")]
        [HttpGet]
        public async Task<IActionResult> RedisGetTest()
        {
            var watch = Stopwatch.StartNew();

            var db = _redis.GetDatabase();
            var foo = await db.StringGetAsync("foo");

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            return Ok(new {result = foo.ToString(), time = elapsedMs});
            // var pong = await db.PingAsync();
        }
    }
}
