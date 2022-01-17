using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using weather_backend.Services;

namespace weather_backend.Controllers;

[ApiController]
[Route("[controller]")]
public class GeolocationController : ControllerBase
{
    private readonly GeolocationService _geolocationService;

    public GeolocationController(GeolocationService geolocationService)
    {
        _geolocationService = geolocationService;
    }

    [HttpGet]
    [Route("ipaddress")]
    public async Task<string> GetIpAddress()
    {
        return await _geolocationService.GetIpAddress();
    }

    [HttpGet]
    [Route("location")]
    public async Task<string> GetLocation()
    {
        return await _geolocationService.GetLocation();
    }
}