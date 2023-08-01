using System;
using System.Buffers.Text;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace weather_backend.Controllers
{
    [Route("api/stringbase64")]
    [ApiController]
    public sealed class StringBase64Controller: ControllerBase
    {
        [HttpGet]
        [Route("decodebase64")]
        public ActionResult<string> Decode([FromQuery] string stringToDecode)
        {
            byte[] bytes = Convert.FromBase64String(stringToDecode);
            string decodedString = Encoding.UTF8.GetString(bytes);
            return Ok(decodedString);
        }
        
        [HttpGet]
        [Route("encodebase64")]
        public ActionResult<string> Encode([FromQuery] string stringToEncode)
        {
            var bytesString = Encoding.UTF8.GetBytes(stringToEncode);
            return Ok(Convert.ToBase64String(bytesString));
        }
    }
}
