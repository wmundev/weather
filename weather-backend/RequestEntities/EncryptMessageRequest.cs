using Microsoft.AspNetCore.Mvc;

namespace weather_backend.RequestEntities
{
    public record EncryptMessageRequest
    {
        [FromQuery] public required string Message { get; init; }
    }
}
