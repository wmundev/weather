using Amazon.DynamoDBv2.DataModel;

namespace weather_backend.Dto
{
    [DynamoDBTable("Music")]
    public class MusicDto
    {
        [DynamoDBHashKey] public string Artist { get; set; }

        [DynamoDBRangeKey("SongTitle")] public string SongTitle { get; set; }
    }
}