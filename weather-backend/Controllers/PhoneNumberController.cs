using System;
using Microsoft.AspNetCore.Mvc;
using PhoneNumbers;
using weather_backend.Models.PhoneService;
using weather_backend.Services.Interfaces;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("phone-number")]
    [ProducesResponseType(typeof(ProblemDetails), 429)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
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
        /// <response code="400">Returns the parser's error message if the phone number cannot be parsed.</response>
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
            // Only the parser's own rejection is the caller's fault, and its message is written to be
            // shown to them. Anything else is a server fault and goes to GlobalExceptionHandler, which
            // does not echo exception text back.
            catch (NumberParseException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
