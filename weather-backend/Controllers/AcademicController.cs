using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using weather_backend.Models;
using weather_backend.Services.Interfaces;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [ProducesResponseType(typeof(ProblemDetails), 429)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public class AcademicController : ControllerBase
    {
        private readonly IAcademicService? _academicService;

        /// <param name="academicService">
        /// Null when <c>ConnectionStrings:Academic</c> is not configured. Optional so that the API starts
        /// without Postgres; the route answers 503 instead.
        /// </param>
        public AcademicController(IAcademicService? academicService = null)
        {
            _academicService = academicService;
        }

        /// <summary>
        /// Retrieves academic information by the specified ID.
        /// </summary>
        /// <param name="id">The ID of the academic to retrieve information for.</param>
        /// <param name="cancellationToken">The request's abort token.</param>
        /// <returns>The <see cref="Academic"/> record.</returns>
        /// <response code="200">Returns the academic.</response>
        /// <response code="404">If no academic has that ID.</response>
        /// <response code="503">If the academic database is not configured.</response>
        [HttpGet]
        [Route("/academic")]
        [ProducesResponseType(typeof(Academic), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<Academic>> GetAcademicById([FromQuery, Required] int id, CancellationToken cancellationToken)
        {
            if (_academicService is null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
                {
                    Status = StatusCodes.Status503ServiceUnavailable,
                    Title = "Academic database is not configured.",
                    Detail = "Set ConnectionStrings:Academic to enable this endpoint."
                });
            }

            var academic = await _academicService.GetAcademicByIdAsync(id, cancellationToken);
            if (academic is null)
            {
                return NotFound();
            }

            return Ok(academic);
        }
    }
}
