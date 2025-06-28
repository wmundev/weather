using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using weather_backend.Services.Interfaces;

namespace weather_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanguageTranslatorController : ControllerBase
    {
        private readonly ILanguageTranslatorService _languageTranslatorService;

        public LanguageTranslatorController(ILanguageTranslatorService languageTranslatorService)
        {
            _languageTranslatorService = languageTranslatorService ?? throw new ArgumentNullException(nameof(languageTranslatorService));
        }

        [HttpPost]
        [Route("generate")]
        public async Task GenerateLanguageFile()
        {
            await Task.CompletedTask;
            // uncomment to use translation
            // await _languageTranslatorService.GenerateLanguageFileAsync();
        }
    }
}
