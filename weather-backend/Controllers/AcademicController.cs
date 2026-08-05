using Microsoft.AspNetCore.Mvc;
using weather_backend.Models;
using weather_backend.Services.Interfaces;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AcademicController : ControllerBase
    {
        private readonly IAcademicService _academicService;

        public AcademicController(IAcademicService academicService)
        {
            _academicService = academicService;
        }

        /// <summary>
        /// Retrieves academic information by the specified ID.
        /// </summary>
        /// <param name="id">The ID of the academic to retrieve information for.</param>
        /// <returns>
        /// An <see cref="ActionResult"/> containing the <see cref="Academic"/> information.
        /// </returns>
        [HttpGet]
        [Route("/academic")]
        [ProducesResponseType(typeof(Academic), 200)]
        [ProducesResponseType(404)]
        public ActionResult<Academic> GetAcademicById(int id)
        {
            var academic = _academicService.GetAcademicById(id);
            if (academic is null)
            {
                return NotFound();
            }

            return Ok(academic);
        }
    }
}
