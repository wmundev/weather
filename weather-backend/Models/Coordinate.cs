using Amazon.DynamoDBv2.DataModel;
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

    [JsonProperty("lon")]
    [DynamoDBProperty("lon")]
    public double Longitude { get; set; }

    [JsonProperty("lat")]
    [DynamoDBProperty("lat")]
    public double Latitude { get; set; }
}