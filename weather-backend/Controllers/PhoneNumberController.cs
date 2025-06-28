using System;
using Microsoft.AspNetCore.Mvc;
using weather_backend.Models.PhoneService;
using weather_backend.Services.Interfaces;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("phone-number")]
    public class PhoneNumberController : ControllerBase
    {
        private readonly IPhoneService _phoneService;

        public PhoneNumberController(IPhoneService phoneService)
        {
            _phoneService = phoneService ?? throw new ArgumentNullException(nameof(phoneService));
        }

        /// <summary>
        /// Validates the provided phone number.
        /// </summary>
        /// <param name="phone">The phone number to validate.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the validation result if successful,
        /// or an error message if the validation fails.
        /// </returns>
        /// <response code="200">Returns the validation result.</response>
        /// <response code="400">Returns an error message if the validation fails.</response>
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
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
