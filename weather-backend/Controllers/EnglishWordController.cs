using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace weather_backend.Controllers
{
    [Route("api/v1/word")]
    [ApiController]
    public sealed class EnglishWordController : ControllerBase
    {
        private readonly string[] _listOfConjunctions = {"and", "or", "but", "nor", "so", "for", "yet"};

        /// <summary>
        /// Capitalizes the first letter of each word in the input string, except for conjunctions.
        /// </summary>
        /// <param name="input">The input string to process.</param>
        /// <returns>
        /// An <see cref="ActionResult{String}"/> containing the processed string with the first letter of each word capitalized, except for conjunctions.
        /// </returns>
        [HttpGet]
        [Route("capitalize-first-word")]
        public ActionResult<string> CapitalizeFirstWord([FromQuery] string input)
        {
            const char separator = ' ';
            var stringArrayByWhitespace = input.Split(separator);
            var capitalisedStringPerWord = stringArrayByWhitespace.Select(word =>
            {
                if (_listOfConjunctions.Contains(word))
                {
                    return word;
                }

                return string.Concat(word.First().ToString().ToUpper(), word.AsSpan(1));
            });

            return Ok(string.Join(separator, capitalisedStringPerWord));
        }
    }
}
