using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
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
            const int maxAttempts = 3;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
                try
                {
                    await _amazonDynamoDbClient.SaveAsync(obj);
                    return;
                }
                catch (ProvisionedThroughputExceededException exception) when (attempt < maxAttempts)
                {
                    // Task.Delay, not Thread.Sleep: this runs on a thread-pool thread that other requests need.
                    var backoff = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
                    _logger.LogWarning(exception, "DynamoDB throughput exceeded saving {CityName}, retrying in {Backoff}", obj.Name, backoff);
                    await Task.Delay(backoff);
                }

            // Any other AWS failure, and throttling on the final attempt, propagate to the caller - the
            // previous version logged and returned, which let callers treat an unsaved record as saved.
        }
    }
}
