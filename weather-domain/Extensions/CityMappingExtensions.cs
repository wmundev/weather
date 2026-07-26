using weather_domain.DatabaseEntities;
using weather_domain.Entities;

namespace weather_domain.Extensions;

public static class CityMappingExtensions
{
    public static DynamoDbCity ToDynamoDbCity(this City city)
    {
        return new DynamoDbCity
        {
            Id = city.Id.ToString(),
            Name = city.Name,
            State = city.State,
            Country = city.Country,
            Coordinate = city.Coordinate
        };
    }
}
