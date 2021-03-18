using System;
using Microsoft.AspNetCore.Mvc;
using weather_backend.Models;
using weather_backend.Services;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AcademicController
    {
        private readonly AcademicService _academicService;
        public AcademicController(AcademicService academicService)
        {
            _academicService = academicService;
        }

        [HttpGet]
        [Route("/academic")]
        public Academic GetAcademicById(int id)
        {
            return _academicService.GetAcademicById(id);
        }
    }
}