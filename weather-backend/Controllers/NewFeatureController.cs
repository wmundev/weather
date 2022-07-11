using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("feature")]
    public class NewFeatureController : ControllerBase
    {
        [Route("pattern")]
        public async Task<ActionResult> Pattern()
        {
            var greeting = "nice one";

            if (greeting is string thisisgreeting) Console.WriteLine(thisisgreeting);

            return Ok();
        }

        [Route("password")]
        public async Task<ActionResult> GeneratePassword()
        {
            return Ok();
        }
    }
}