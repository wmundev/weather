namespace weather_backend.RequestEntities
{
    public record DecryptMessageRequest
    {
        public required string Message { get; init; }
        public required string Key { get; init; }
        public required string Nonce { get; init; }
    }
}
