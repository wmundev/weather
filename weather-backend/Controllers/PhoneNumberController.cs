using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using weather_backend.Services;

namespace weather_backend.Controllers;

[ApiController]
[Route("[controller]")]
public class PhoneNumberController : ControllerBase
{
    private readonly IPhoneService _phoneService;

    public PhoneNumberController(IPhoneService phoneService)
    {
        _phoneService = phoneService;
    }

    [HttpGet]
    [Route("/phone")]
    public async Task<ActionResult> ValidatePhoneNumber([FromQuery(Name = "phone")] string phone)
    {
        return Ok(_phoneService.ValidatePhoneNumber(phone));
    }
}