using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ConfigCat.Client;
using Microsoft.AspNetCore.Mvc;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("api/prime-number")]
    [ProducesResponseType(typeof(ProblemDetails), 429)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public class PrimeNumberController : ControllerBase
    {
        private readonly IConfigCatClient _configCatClient;

        public PrimeNumberController(IConfigCatClient configCatClient)
        {
            _configCatClient = configCatClient ?? throw new ArgumentNullException(nameof(configCatClient));
        }

        /// <summary>
        /// Determines if the provided number is a prime number.
        /// </summary>
        /// <param name="number">The number to check for primality.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing <c>true</c> if the number is prime, <c>false</c> otherwise.
        /// </returns>
        /// <response code="200">Returns the primality result.</response>
        /// <response code="400">Returns an error message if the feature is not enabled.</response>
        [HttpGet]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> Get([FromQuery(Name = "number")] int number)
        {
            var isMyAwesomeFeatureEnabled = await _configCatClient.GetValueAsync("primenumber", false);
            if (!isMyAwesomeFeatureEnabled)
            {
                return BadRequest("Not enabled");
            }

            // Primes are greater than 1 by definition; 0 and negatives used to fall through as prime.
            if (number < 2)
            {
                return Ok(false);
            }

            // Trial division only needs to reach the square root: any factor above it pairs with one
            // below. Counting all the way to the number itself kept a request thread busy for billions of
            // iterations on a large prime. i <= number / i is the overflow-safe form of i * i <= number.
            var isPrime = true;
            for (var i = 2; i <= number / i; i++)
            {
                if (number % i == 0)
                {
                    isPrime = false;
                    break;
                }
            }

            return Ok(isPrime);
        }

        /// <summary>
        /// Iterates over a list of random integers to find the maximum value.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the maximum value found in the list.
        /// </returns>
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
