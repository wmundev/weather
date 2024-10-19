using System.Text;
using weather_application.Services;
using weather_application.Services.Interfaces;
using Xunit;

namespace weather_test.Services
{
    public class EncryptionServiceTest
    {
        private readonly IEncryptionService _encryptionService;

        public EncryptionServiceTest()
        {
            _encryptionService = new EncryptionService();
        }

        [Fact]
        public void EncryptDecryptTest()
        {
            var text = Encoding.UTF8.GetBytes("Got more soul than a sock with a hole");

            var (ciphertext, nonce, key) = _encryptionService.Encrypt(text);
            var decrypted = _encryptionService.Decrypt(ciphertext, nonce, key);
            Assert.Equal(text, decrypted);
        }
    }
}
