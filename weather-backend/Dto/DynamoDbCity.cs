using Amazon.DynamoDBv2.DataModel;
using weather_backend.Models;

namespace weather_backend.Dto
{
    [DynamoDBTable("weather")]
    public class DynamoDbCity
    {
        [DynamoDBProperty("id")] public required string Id { get; init; }

        [DynamoDBHashKey]
        [DynamoDBProperty("name")]
        public required string Name { get; init; }

        [DynamoDBProperty("state")] public required string State { get; init; }

        [DynamoDBProperty("country")] public required string Country { get; init; }

        [DynamoDBProperty("coord")] public required Coordinate Coordinate { get; init; }
    }
}
