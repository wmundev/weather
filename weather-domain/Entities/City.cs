using System.Text.Json.Serialization;

namespace weather_domain.Entities
{
    public class City
    {
        [JsonPropertyName("id")] public required double Id { get; init; }
        [JsonPropertyName("name")] public required string Name { get; init; }
        [JsonPropertyName("state")] public required string State { get; init; }
        [JsonPropertyName("country")] public required string Country { get; init; }
        [JsonPropertyName("coord")] public required Coordinate Coordinate { get; init; }
    }
}
