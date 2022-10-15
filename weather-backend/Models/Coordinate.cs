using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;

namespace weather_backend.Models
{
    public class Coordinate
    {
        [JsonPropertyName("lon")]
        [DynamoDBProperty("lon")]
        public double Longitude { get; init; }

        [JsonPropertyName("lat")]
        [DynamoDBProperty("lat")]
        public double Latitude { get; init; }
    }
}
