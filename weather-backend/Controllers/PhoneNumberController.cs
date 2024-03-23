using Microsoft.AspNetCore.Mvc;
using weather_backend.Models.PhoneService;
using weather_backend.Services;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("phone-number")]
    public class PhoneNumberController : ControllerBase
    {
        private readonly IPhoneService _phoneService;

        public PhoneNumberController(IPhoneService phoneService)
        {
            _phoneService = phoneService;
        }

        [HttpGet]
        [Route("phone")]
        [ProducesResponseType(typeof(ValidatePhoneNumberModel), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public IActionResult ValidatePhoneNumber([FromQuery(Name = "phone")] string phone)
        {
            try
            {
                var validationResult = _phoneService.ValidatePhoneNumber(phone);
                return Ok(validationResult);
            }
            catch (System.Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
