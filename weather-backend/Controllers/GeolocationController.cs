using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using weather_backend.Services;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("geolocation")]
    public class GeolocationController : ControllerBase
    {
        private readonly IGeolocationService _geolocationService;

        public GeolocationController(IGeolocationService geolocationService)
        {
            _geolocationService = geolocationService ?? throw new ArgumentNullException(nameof(geolocationService));
        }

        /// <summary>
        /// Retrieves the IP address of the client.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{ActionResult{String}}"/> representing the asynchronous operation,
        /// containing the client's IP address as a string.
        /// </returns>
        [HttpGet]
        [Route("ipaddress")]
        public async Task<ActionResult<string>> GetIpAddress()
        {
            return await _geolocationService.GetIpAddress();
        }

        /// <summary>
        /// Retrieves the geolocation information of the client.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{ActionResult{String}}"/> representing the asynchronous operation,
        /// containing the client's geolocation information as a string.
        /// </returns>
        [HttpGet]
        [Route("location")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<string>> GetLocation()
        {
            var location = await _geolocationService.GetLocation();
            if (location is null)
            {
                return BadRequest(new ProblemDetails {Title = "Unknown client IP address", Detail = "The remote IP address of this request is not available."});
            }

            return Ok(location);
        }
    }
}
