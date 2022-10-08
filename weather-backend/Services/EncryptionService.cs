using System.Security.Cryptography;
using Sodium;

namespace weather_backend.Services
{
    public class EncryptionService : IEncryptionService
    {
        public (byte[] ciphertext, byte[] nonce, byte[] key) Encrypt(byte[] textToEncrypt)
        {
            var key = new byte[32];
            RandomNumberGenerator.Create().GetBytes(key);

            var nonce = new byte[24];
            RandomNumberGenerator.Create().GetBytes(nonce);

            var ciphertext = SecretAeadXChaCha20Poly1305.Encrypt(textToEncrypt, nonce, key);

            return (ciphertext, nonce, key);
        }

        public byte[] Decrypt(byte[] text, byte[] nonce, byte[] key)
        {
            return SecretAeadXChaCha20Poly1305.Decrypt(text, nonce, key);
        }
    }
}