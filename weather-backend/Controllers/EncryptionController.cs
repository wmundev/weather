using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using weather_application.Services.Interfaces;
using weather_backend.RequestEntities;
using weather_domain.ResponseEntities;

namespace weather_backend.Controllers
{
    [Route("api/encryption")]
    [ApiController]
    public class EncryptionController : ControllerBase
    {
        private readonly IEncryptionService _encryptionService;

        public EncryptionController(IEncryptionService encryptionService)
        {
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        }

        /// <summary>
        /// Encrypts a predefined message and returns the encrypted result as a Base64 string.
        /// </summary>
        /// <returns>
        /// A Base64 encoded string representing the encrypted message.
        /// </returns>
        [HttpGet]
        [Route("encrypt")]
        public EncryptMessageResponse EncryptMessage(EncryptMessageRequest request)
        {
            var message = Encoding.ASCII.GetBytes(request.Message);
            var (encryptedResult, nonce, key) = _encryptionService.Encrypt(message);
            return new EncryptMessageResponse {EncryptedMessage = Convert.ToBase64String(encryptedResult), Nonce = Convert.ToBase64String(nonce), Key = Convert.ToBase64String(key)};
        }

        [HttpPost]
        [Route("decrypt")]
        public DecryptMessageResponse DecryptMessage([FromBody] DecryptMessageRequest request)
        {
            var encryptedMessage = Convert.FromBase64String(request.Message);
            var nonce = Convert.FromBase64String(request.Nonce);
            var key = Convert.FromBase64String(request.Key);
            var decryptedMessage = _encryptionService.Decrypt(encryptedMessage, nonce, key);
            return new DecryptMessageResponse {Message = Encoding.ASCII.GetString(decryptedMessage)};
        }
    }
}
