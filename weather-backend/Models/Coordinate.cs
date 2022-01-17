using Newtonsoft.Json;

namespace weather_backend.Models;

public class Coordinate
{
    public Coordinate()
    {
    }

    public Coordinate(double longitude, double latitude)
    {
        Longitude = longitude;
        Latitude = latitude;
    }

    [JsonProperty("lon")] public double Longitude { get; set; }

    [JsonProperty("lat")] public double Latitude { get; set; }
}