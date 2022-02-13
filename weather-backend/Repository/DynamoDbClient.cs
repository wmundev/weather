using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using weather_backend.Dto;

namespace weather_backend.Repository;

public class DynamoDbClient
{
    private readonly IDynamoDBContext _amazonDynamoDbClient;

    public DynamoDbClient(IDynamoDBContext amazonDynamoDbClient)
    {
        _amazonDynamoDbClient = amazonDynamoDbClient;
    }

    public async Task<MusicDto> getthings()
    {
        return await _amazonDynamoDbClient.LoadAsync<MusicDto>("Dream Theater", "Surrounded");
    }

    public async Task saveRecord(DynamoDbCity obj)
    {
        await _amazonDynamoDbClient.SaveAsync(obj);
    }
}