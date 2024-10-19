namespace weather_domain.ResponseEntities
{
    public class EncryptMessageResponse
    {
        public required string EncryptedMessage { get; init; }
        public required string Nonce { get; init; }
        public required string Key { get; init; }
    }
}
