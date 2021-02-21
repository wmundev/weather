using Newtonsoft.Json;

namespace weather_backend.Models
{
    public class WeatherSystem
    {
        [JsonProperty("type")] public int type { get; set; }
        [JsonProperty("id")] public int id { get; set; }
        [JsonProperty("country")] public string country { get; set; }
        [JsonProperty("sunrise")] public int sunrise { get; set; }
        [JsonProperty("sunset")] public int sunset { get; set; }
    }
}