using Microsoft.AspNetCore.Mvc;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("hello-world")]
    public class HelloWorldController : ControllerBase
    {
        /// <summary>
        /// Returns a simple "Hello World" message.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the "Hello World" message.
        /// </returns>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello World");
        }
    }
}
