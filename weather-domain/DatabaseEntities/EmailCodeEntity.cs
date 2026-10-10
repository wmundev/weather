using Amazon.DynamoDBv2.DataModel;

namespace weather_domain.DatabaseEntities
{
    [DynamoDBTable("EmailCode")]
    public sealed class EmailCodeEntity
    {
        [DynamoDBHashKey] public int Id { get; init; }
        public string? Code { get; init; }
        public string? Email { get; init; }
    }
}
