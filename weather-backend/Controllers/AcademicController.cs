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

        [HttpGet]
        [Route("/academic")]
        public ActionResult<Academic> GetAcademicById(int id)
        {
            return Ok(_academicService.GetAcademicById(id));
        }
    }
}