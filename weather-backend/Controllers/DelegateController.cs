using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using weather_backend.Services;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("api/delegates")]
    [ProducesResponseType(typeof(ProblemDetails), 429)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public class DelegateController : ControllerBase
    {
        private readonly DelegateService _delegateService;
        private readonly ILogger<DelegateController> _logger;

        public DelegateController(DelegateService delegateService, ILogger<DelegateController> logger)
        {
            _delegateService = delegateService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Tests the delegate functionality by sorting items and printing the sum of each pair.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the operation.
        /// </returns>
        [HttpGet]
        public IActionResult TestDelegate()
        {
            _delegateService.SortThings((i, i1) => { _logger.LogDebug("Delegate pair sum {PairSum}", i + i1); });
            return Ok();
        }
    }
}
