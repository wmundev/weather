using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Logging;
using weather_backend.Dto;
using weather_domain.DatabaseEntities;

namespace weather_backend.Repository
{
    public interface IDynamoDbClient
    {
        Task<MusicDto> LoadMusicDto(CancellationToken token = default);
        Task SaveRecord(DynamoDbCity obj);
        Task<EmailCodeEntity> LoadEmailCode();
    }

    public class DynamoDbClient : IDynamoDbClient
    {
        private readonly IDynamoDBContext _amazonDynamoDbClient;
        private readonly ILogger<DynamoDbClient> _logger;

        public DynamoDbClient(ILogger<DynamoDbClient> logger, IDynamoDBContext amazonDynamoDbClient)
        {
            _logger = logger;
            _amazonDynamoDbClient = amazonDynamoDbClient;
        }

        public async Task<EmailCodeEntity> LoadEmailCode()
        {
            return await _amazonDynamoDbClient.LoadAsync<EmailCodeEntity>(1177876938);
        }

        public async Task<MusicDto> LoadMusicDto(CancellationToken token = default)
        {
            return await _amazonDynamoDbClient.LoadAsync<MusicDto>("Dream Theater", "Surrounded", token);
        }

        public async Task SaveRecord(DynamoDbCity obj)
        {
            var retry = 0;
            var maxRetry = 3;
            while (retry < maxRetry)
                try
                {
                    await _amazonDynamoDbClient.SaveAsync(obj);
                    break;
                }
                catch (ProvisionedThroughputExceededException throughputExceededException)
                {
                    _logger.LogError("throughput exceeded, retrying...");
                    _logger.LogError("Error Message:  " + throughputExceededException.Message);
                    retry += 1;
                    Thread.Sleep(1000);
                }
                catch (AmazonServiceException ase)
                {
                    _logger.LogError("Could not complete operation");
                    _logger.LogError("Error Message:  " + ase.Message);
                    _logger.LogError("HTTP Status:    " + ase.StatusCode);
                    _logger.LogError("AWS Error Code: " + ase.ErrorCode);
                    _logger.LogError("Error Type:     " + ase.ErrorType);
                    _logger.LogError("Request ID:     " + ase.RequestId);
                    break;
                }
                catch (AmazonClientException ace)
                {
                    _logger.LogError("Internal error occurred communicating with DynamoDB");
                    _logger.LogError("Error Message:  " + ace.Message);
                    break;
                }
        }
    }
}
