using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace weather_backend.Controllers
{
    [Route("api/stringbase64")]
    [ApiController]
    [ProducesResponseType(typeof(ProblemDetails), 429)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public sealed class StringBase64Controller : ControllerBase
    {
        /// <summary>
        /// Decodes a Base64 encoded string.
        /// </summary>
        /// <param name="stringToDecode">The Base64 encoded string to decode.</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing the decoded string.
        /// </returns>
        /// <response code="200">Returns the decoded string.</response>
        /// <response code="400">If the input is not valid Base64.</response>
        [HttpGet]
        [Route("decodebase64")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public ActionResult<string> Decode([FromQuery] string stringToDecode)
        {
            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(stringToDecode);
            }
            catch (FormatException)
            {
                // Malformed input is the caller's mistake, not a server fault.
                return BadRequest(new ProblemDetails {Title = "Invalid Base64.", Detail = "stringToDecode is not a valid Base64 string."});
            }

            string decodedString = Encoding.UTF8.GetString(bytes);
            return Ok(decodedString);
        }

        /// <summary>
        /// Encodes a string to Base64 format.
        /// </summary>
        /// <param name="stringToEncode">The string to encode to Base64 format.</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing the Base64 encoded string.
        /// </returns>
        [HttpGet]
        [Route("encodebase64")]
        public ActionResult<string> Encode([FromQuery] string stringToEncode)
        {
            var bytesString = Encoding.UTF8.GetBytes(stringToEncode);
            return Ok(Convert.ToBase64String(bytesString));
        }
    }
}
