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

        [HttpGet]
        [Route("ipaddress")]
        public async Task<ActionResult<string>> GetIpAddress()
        {
            return await _geolocationService.GetIpAddress();
        }

        [HttpGet]
        [Route("location")]
        public async Task<ActionResult<string>> GetLocation()
        {
            return await _geolocationService.GetLocation();
        }
    }
}
