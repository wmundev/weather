using Newtonsoft.Json;

namespace weather_backend.Models
{
    public class Cloud
    {
        [JsonProperty("all")] public int all { get; set; }
    }
}