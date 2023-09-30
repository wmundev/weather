using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace weather_backend.Controllers
{
    [Route("api/v1/word")]
    [ApiController]
    public sealed class EnglishWordController : ControllerBase
    {
        private readonly string[] listOfConjunctions = { "and", "or", "but", "nor", "so", "for", "yet" };

        [HttpGet]
        [Route("capitalize-first-word")]
        public ActionResult<string> CapitalizeFirstWord([FromQuery] string input)
        {
            const char separator = ' ';
            var stringArrayByWhitespace = input.Split(separator);
            var capitalisedStringPerWord = stringArrayByWhitespace.Select(word =>
            {
                if (listOfConjunctions.Contains(word))
                {
                    return word;
                }

                return string.Concat(word.First().ToString().ToUpper(), word.AsSpan(1));
            });

            return Ok(string.Join(separator, capitalisedStringPerWord));
        }
    }
}
