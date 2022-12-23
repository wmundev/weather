using Amazon.DynamoDBv2.DataModel;

namespace weather_backend.Dto
{
    [DynamoDBTable("Music")]
    public class MusicDto
    {
        [DynamoDBHashKey] public required string Artist { get; set; }

        [DynamoDBRangeKey("SongTitle")] public required string SongTitle { get; set; }
    }
}
