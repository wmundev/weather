namespace weather_application.Services.Interfaces
{
    public interface IEncryptionService
    {
        (byte[] ciphertext, byte[] nonce, byte[] key) Encrypt(byte[] textToEncrypt);
        byte[] Decrypt(byte[] text, byte[] nonce, byte[] key);
    }
}
