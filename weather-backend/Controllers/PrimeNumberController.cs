using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("api/prime-number")]
    public class PrimeNumberController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult Get([FromQuery(Name = "number")] int number)
        {
            if (number == 1)
            {
                return Ok("false");
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

        [HttpGet("haha")]
        public IActionResult FastestIteratingOverList()
        {
            Random rng = new(80000);
            var Size = 10000;
            var items = Enumerable.Range(1, Size).Select(x => rng.Next()).ToList();

            int max = 0;
            foreach (var item in CollectionsMarshal.AsSpan(items))
            {
                if (max < item)
                {
                    max = item;
                }
            }

            return Ok(max);
        }
    }
}
