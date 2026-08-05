using System.Globalization;
using weather_domain.DatabaseEntities;
using weather_domain.Entities;

namespace weather_domain.Extensions;

public static class CityMappingExtensions
{
    public static DynamoDbCity ToDynamoDbCity(this City city)
    {
        return new DynamoDbCity
        {
            // Invariant culture: this value becomes a persisted DynamoDB attribute and must not vary by locale.
            Id = city.Id.ToString(CultureInfo.InvariantCulture),
            Name = city.Name,
            State = city.State,
            Country = city.Country,
            Coordinate = city.Coordinate
        };
    }
}
