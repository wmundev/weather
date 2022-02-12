using System;
using System.Globalization;
using Amazon.DynamoDBv2.DataModel;
using weather_backend.Models;

namespace weather_backend.Dto;

[DynamoDBTable("weather")]
public class DynamoDbCity
{
    public DynamoDbCity()
    {
    }

    public DynamoDbCity(double id, string name, string state, string country, Coordinate coordinate)
    {
        Id = Convert.ToString(id, CultureInfo.InvariantCulture);
        Name = name;
        State = state;
        Country = country;
        Coordinate = coordinate;
    }


    [DynamoDBProperty("id")] public string Id { get; set; }

    [DynamoDBHashKey]
    [DynamoDBProperty("name")]
    public string Name { get; set; }

    [DynamoDBProperty("state")] public string State { get; set; }

    [DynamoDBProperty("country")] public string Country { get; set; }

    [DynamoDBProperty("coord")] public Coordinate Coordinate { get; set; }
}