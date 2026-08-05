using System.Text.Json.Serialization;

namespace weather_backend.Models
{
    public class MainWeather
    {
        [JsonPropertyName("temp")] public double temp { get; set; }

        [JsonPropertyName("feels_like")] public double feels_like { get; set; }

        [JsonPropertyName("temp_min")] public double temp_min { get; set; }

        [JsonPropertyName("temp_max")] public double temp_max { get; set; }

        [JsonPropertyName("pressure")] public int pressure { get; set; }

        [JsonPropertyName("humidity")] public int humidity { get; set; }

        [JsonPropertyName("sea_level")] public int? sea_level { get; set; }

        [JsonPropertyName("grnd_level")] public int? grnd_level { get; set; }
    }
}
