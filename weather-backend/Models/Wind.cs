using System.Text.Json.Serialization;

namespace weather_backend.Models
{
    public class Wind
    {
        [JsonPropertyName("speed")] public double speed { get; set; }

        [JsonPropertyName("deg")] public int deg { get; set; }

        [JsonPropertyName("gust")] public double? gust { get; set; }
    }
}
