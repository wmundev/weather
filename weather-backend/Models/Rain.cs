using Newtonsoft.Json;

namespace weather_backend.Models
{
    public class Rain
    {
        [JsonProperty("1h")] public double? OneHour { get; set; }

        [JsonProperty("3h")] public double? ThreeHours { get; set; }
    }
}
