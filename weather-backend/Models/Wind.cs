using Newtonsoft.Json;

namespace weather_backend.Models
{
    public class Wind
    {
        [JsonProperty("speed")]
        public double speed{ get; set; }
        [JsonProperty("deg")]
        public int deg{ get; set; }
    }
}