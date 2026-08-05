using System.Text.Json.Serialization;

namespace weather_backend.Models
{
    public class Cloud
    {
        [JsonPropertyName("all")] public int all { get; set; }
    }
}