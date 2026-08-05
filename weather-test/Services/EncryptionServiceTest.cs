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

        [Theory]
        [InlineData("café")]
        [InlineData("日本語のテキスト")]
        [InlineData("emoji \U0001F327 and accents: àéîõü")]
        public void EncryptDecrypt_RoundTripsNonAsciiText(string original)
        {
            // The controller used Encoding.ASCII, which replaced every non-ASCII character with '?'
            // before the text ever reached this service.
            var text = Encoding.UTF8.GetBytes(original);

            var (ciphertext, nonce, key) = _encryptionService.Encrypt(text);
            var decrypted = _encryptionService.Decrypt(ciphertext, nonce, key);

            Assert.Equal(original, Encoding.UTF8.GetString(decrypted));
        }

        [Fact]
        public void Encrypt_UsesAFreshKeyAndNonceEachTime()
        {
            var text = Encoding.UTF8.GetBytes("same message");

            var first = _encryptionService.Encrypt(text);
            var second = _encryptionService.Encrypt(text);

            Assert.NotEqual(first.key, second.key);
            Assert.NotEqual(first.nonce, second.nonce);
            Assert.NotEqual(first.ciphertext, second.ciphertext);
        }
    }
}
