using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace weather_backend.Controllers
{
    [Route("api/stringbase64")]
    [ApiController]
    public sealed class StringBase64Controller : ControllerBase
    {
        /// <summary>
        /// Decodes a Base64 encoded string.
        /// </summary>
        /// <param name="stringToDecode">The Base64 encoded string to decode.</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing the decoded string.
        /// </returns>
        [HttpGet]
        [Route("decodebase64")]
        public ActionResult<string> Decode([FromQuery] string stringToDecode)
        {
            byte[] bytes = Convert.FromBase64String(stringToDecode);
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
