using Microsoft.AspNetCore.Mvc;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrimeNumberController : ControllerBase
    {

        [HttpGet]
        public IActionResult Get([FromQuery(Name = "number")] int number)
        {
            if (number < 2)
            {
                return BadRequest("Number must be greater than 1");
            }

            var isPrime = true;
            for (var i = 2; i < number; i++)
            {
                if (number % i == 0)
                {
                    isPrime = false;
                    break;
                }
            }

            return Ok(isPrime);
        }
    }
}
