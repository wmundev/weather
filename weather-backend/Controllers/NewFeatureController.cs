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

        [Route("pattern")]
        [HttpGet]
        public IActionResult Pattern()
        {
            var greeting = "nice one";

            if (greeting is { } thisisgreeting) Console.WriteLine(thisisgreeting);

            return Ok();
        }

        [Route("password")]
        [HttpGet]
        public IActionResult GeneratePassword()
        {
            return Ok();
        }

        [Route("redis")]
        [HttpPost]
        public async Task<IActionResult> RedisSaveTest([FromBody] RedisSaveTestDto value)
        {
            var db = _redis.GetDatabase();
            var foo = await db.StringSetAsync("foo", value.Value);
            return Ok(foo.ToString());
            // var pong = await db.PingAsync();
            // return Ok();
        }

        [Route("redis")]
        [HttpGet]
        public async Task<IActionResult> RedisGetTest()
        {
            var watch = Stopwatch.StartNew();

            var db = _redis.GetDatabase();
            var foo = await db.StringGetAsync("foo");

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            return Ok(new { result = foo.ToString(), time = elapsedMs });
            // var pong = await db.PingAsync();
            // return Ok();
        }
    }
}
