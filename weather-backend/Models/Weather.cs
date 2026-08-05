using System.Text.Json.Serialization;

namespace weather_backend.Models
{
    public class Weather
    {
        [JsonPropertyName("id")] public required int id { get; set; }

        [JsonPropertyName("main")] public required string main { get; set; }

        [JsonPropertyName("description")] public required string description { get; set; }

        [JsonPropertyName("icon")] public required string icon { get; set; }
    }
}
