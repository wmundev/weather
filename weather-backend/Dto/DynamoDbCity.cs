using System;
using System.Globalization;
using Amazon.DynamoDBv2.DataModel;
using weather_backend.Models;

namespace weather_backend.Dto
{
    [DynamoDBTable("weather")]
    public class DynamoDbCity
    {
        public DynamoDbCity(double id, string name, string state, string country, Coordinate coordinate)
        {
            Id = Convert.ToString(id, CultureInfo.InvariantCulture);
            Name = name;
            State = state;
            Country = country;
            Coordinate = coordinate;
        }


        [DynamoDBProperty("id")] public required string Id { get; set; }

        [DynamoDBHashKey]
        [DynamoDBProperty("name")]
        public required string Name { get; set; }

        [DynamoDBProperty("state")] public required string State { get; set; }

        [DynamoDBProperty("country")] public required string Country { get; set; }

        [DynamoDBProperty("coord")] public required Coordinate Coordinate { get; set; }
    }
}
