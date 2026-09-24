using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using weather_application.Services.Interfaces;
using weather_backend.RequestEntities;
using weather_domain.ResponseEntities;

namespace weather_backend.Controllers
{
    [Route("api/encryption")]
    [ApiController]
    [ProducesResponseType(typeof(ProblemDetails), 429)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
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
            var message = Encoding.UTF8.GetBytes(request.Message);
            var (encryptedResult, nonce, key) = _encryptionService.Encrypt(message);
            return new EncryptMessageResponse {EncryptedMessage = Convert.ToBase64String(encryptedResult), Nonce = Convert.ToBase64String(nonce), Key = Convert.ToBase64String(key)};
        }

        /// <summary>
        /// Decrypts a message produced by <see cref="EncryptMessage"/>.
        /// </summary>
        /// <param name="request">The Base64 ciphertext, nonce and key returned by the encrypt endpoint.</param>
        /// <returns>
        /// A <see cref="DecryptMessageResponse"/> containing the plaintext.
        /// </returns>
        /// <response code="200">Returns the decrypted message.</response>
        /// <response code="400">If a field is not valid Base64, the key or nonce is the wrong length, or the message fails authentication.</response>
        [HttpPost]
        [Route("decrypt")]
        [ProducesResponseType(typeof(DecryptMessageResponse), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public ActionResult<DecryptMessageResponse> DecryptMessage([FromBody] DecryptMessageRequest request)
        {
            byte[] decryptedMessage;
            try
            {
                var encryptedMessage = Convert.FromBase64String(request.Message);
                var nonce = Convert.FromBase64String(request.Nonce);
                var key = Convert.FromBase64String(request.Key);
                decryptedMessage = _encryptionService.Decrypt(encryptedMessage, nonce, key);
            }
            // Every one of these is bad input: FormatException for malformed Base64, the Sodium
            // Key/NonceOutOfRangeException types (ArgumentOutOfRangeException subclasses) for a wrong
            // length, and CryptographicException when the ciphertext does not authenticate against the
            // key and nonce - a tampered message or the wrong key, not a server fault.
            catch (Exception ex) when (ex is FormatException or ArgumentOutOfRangeException or CryptographicException)
            {
                return BadRequest(new ProblemDetails {Title = "Message could not be decrypted.", Detail = "Check that message, nonce and key are the Base64 values returned by the encrypt endpoint."});
            }

            return new DecryptMessageResponse {Message = Encoding.UTF8.GetString(decryptedMessage)};
        }
    }
}
