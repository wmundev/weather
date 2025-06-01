using Amazon.DynamoDBv2.DataModel;

namespace weather_backend.Dto
{
    [DynamoDBTable("EmailCode")]
    public sealed class EmailCodeEntity
    {
        [DynamoDBHashKey] public int Id { get; init; }
        public string? Code { get; init; }
        public string? Email { get; init; }
    }
}
