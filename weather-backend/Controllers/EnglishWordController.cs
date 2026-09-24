using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace weather_backend.Controllers
{
    [Route("api/v1/word")]
    [ApiController]
    [ProducesResponseType(typeof(ProblemDetails), 429)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
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
        /// <response code="200">Returns the capitalised text.</response>
        [HttpGet]
        [Route("capitalize-first-word")]
        [ProducesResponseType(typeof(string), 200)]
        public ActionResult<string> CapitalizeFirstWord([FromQuery] string input)
        {
            const char separator = ' ';
            var stringArrayByWhitespace = input.Split(separator);
            var capitalisedStringPerWord = stringArrayByWhitespace.Select(word =>
            {
                // Consecutive, leading or trailing separators split out empty words; there is nothing to
                // capitalise in them, and First() on an empty string throws.
                if (word.Length == 0 || _listOfConjunctions.Contains(word))
                {
                    return word;
                }

                return string.Concat(word.First().ToString().ToUpper(), word.AsSpan(1));
            });

            return Ok(string.Join(separator, capitalisedStringPerWord));
        }
    }
}
