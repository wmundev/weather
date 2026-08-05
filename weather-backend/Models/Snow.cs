using System.Text.Json.Serialization;

namespace weather_backend.Models
{
    public class Snow
    {
        [JsonPropertyName("1h")] public double? OneHour { get; set; }

        [JsonPropertyName("3h")] public double? ThreeHours { get; set; }
    }
}
