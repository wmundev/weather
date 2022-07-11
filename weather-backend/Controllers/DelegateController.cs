using System;
using System.Threading.Tasks;
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

        [HttpGet]
        [Route("test")]
        public async Task<ActionResult> TestDelegate()
        {
            _delegateService.SortThings((i, i1) => { Console.WriteLine(i + i1); });
            return Ok();
        }
    }
}