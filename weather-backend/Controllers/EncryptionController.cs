using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using weather_backend.Services;

namespace weather_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EncryptionController : ControllerBase
    {
        private readonly IEncryptionService _encryptionService;

        public EncryptionController(IEncryptionService encryptionService)
        {
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        }

        [HttpGet]
        [Route("encrypt")]
        public string Get()
        {
            var message = Encoding.ASCII.GetBytes("Got more soul than a sock with a hole");
            var (encryptedResult, _, _) = _encryptionService.Encrypt(message);
            return Convert.ToBase64String(encryptedResult);
        }
    }
}