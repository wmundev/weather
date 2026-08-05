using System.Security.Cryptography;
using Sodium;
using weather_application.Services.Interfaces;

namespace weather_application.Services
{
    public sealed class EncryptionService : IEncryptionService
    {
        public (byte[] ciphertext, byte[] nonce, byte[] key) Encrypt(byte[] textToEncrypt)
        {
            // RandomNumberGenerator.Fill uses the shared generator, so nothing needs disposing here.
            var key = new byte[32];
            RandomNumberGenerator.Fill(key);

            var nonce = new byte[24];
            RandomNumberGenerator.Fill(nonce);

            var ciphertext = SecretAeadXChaCha20Poly1305.Encrypt(textToEncrypt, nonce, key);

            return (ciphertext, nonce, key);
        }

        public byte[] Decrypt(byte[] text, byte[] nonce, byte[] key)
        {
            return SecretAeadXChaCha20Poly1305.Decrypt(text, nonce, key);
        }
    }
}
