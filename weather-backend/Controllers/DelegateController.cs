using System;
using Microsoft.AspNetCore.Mvc;
using weather_backend.Services;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("delegate")]
    public class DelegateController : ControllerBase
    {
        private readonly DelegateService _delegateService;

        public DelegateController(DelegateService delegateService)
        {
            _delegateService = delegateService;
        }

        /// <summary>
        /// Tests the delegate functionality by sorting items and printing the sum of each pair.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the operation.
        /// </returns>
        [HttpGet]
        [Route("test")]
        public IActionResult TestDelegate()
        {
            _delegateService.SortThings((i, i1) => { Console.WriteLine(i + i1); });
            return Ok();
        }
    }
}
