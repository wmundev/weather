using System.Text.Json.Serialization;

namespace weather_backend.Models
{
    public class WeatherSystem
    {
        [JsonPropertyName("type")] public int type { get; set; }
        [JsonPropertyName("id")] public int id { get; set; }
        [JsonPropertyName("country")] public required string country { get; set; }
        [JsonPropertyName("sunrise")] public required int sunrise { get; set; }
        [JsonPropertyName("sunset")] public required int sunset { get; set; }
    }
}
