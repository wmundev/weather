using Newtonsoft.Json;

namespace weather_backend.Models
{
    public class Coordinate
    {
        [JsonProperty("lon")]
        public double lon { get; set; }
        
        [JsonProperty("lat")]
        public double lat { get; set; }
    }
}