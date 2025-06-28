using Newtonsoft.Json;

namespace weather_backend.Models
{
    public class WeatherSystem
    {
        [JsonProperty("type")] public int type { get; set; }
        [JsonProperty("id")] public int id { get; set; }
        [JsonProperty("country")] public required string country { get; set; }
        [JsonProperty("sunrise")] public required int sunrise { get; set; }
        [JsonProperty("sunset")] public required int sunset { get; set; }
    }
}
