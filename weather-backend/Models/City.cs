using System.Text.Json.Serialization;

namespace weather_backend.Models
{
    public class City
    {
        [JsonPropertyName("id")] public double Id { get; init; }
        [JsonPropertyName("name")] public string Name { get; init; }
        [JsonPropertyName("state")] public string State { get; init; }
        [JsonPropertyName("country")] public string Country { get; init; }
        [JsonPropertyName("coord")] public Coordinate Coordinate { get; init; }
    }
}
