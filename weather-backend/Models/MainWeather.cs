using Newtonsoft.Json;

namespace weather_backend.Models;

public class MainWeather
{
    [JsonProperty("temp")] public double temp { get; set; }

    [JsonProperty("feels_like")] public double feels_like { get; set; }

    [JsonProperty("temp_min")] public double temp_min { get; set; }

    [JsonProperty("temp_max")] public double temp_max { get; set; }

    [JsonProperty("pressure")] public int pressure { get; set; }

    [JsonProperty("humidity")] public int humidity { get; set; }
}