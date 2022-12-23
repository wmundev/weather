using Newtonsoft.Json;

namespace weather_backend.Models
{
    public class Weather
    {
        [JsonProperty("id")] public required int id { get; set; }

        [JsonProperty("main")] public required string main { get; set; }

        [JsonProperty("description")] public required string description { get; set; }

        [JsonProperty("icon")] public required string icon { get; set; }
    }
}
