using Microsoft.AspNetCore.Mvc;
using weather_backend.Models;
using weather_backend.Services;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AcademicController : ControllerBase
    {
        private readonly AcademicService _academicService;

        public AcademicController(AcademicService academicService)
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
        public ActionResult<Academic> GetAcademicById(int id)
        {
            return Ok(_academicService.GetAcademicById(id));
        }
    }
}
