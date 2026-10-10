using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using weather_domain.DatabaseEntities;

namespace weather_repository.DynamoDb
{
    public interface IDynamoDbClient
    {
        Task<MusicDto> LoadMusicDto(CancellationToken token = default);
        Task SaveRecord(DynamoDbCity obj);
        Task<EmailCodeEntity> LoadEmailCode();
    }

    public class DynamoDbClient : IDynamoDbClient
    {
        // The demo tables hold one well-known item each; these are their keys.
        private const int EmailCodeId = 1177876938;
        private const string MusicArtist = "Dream Theater";
        private const string MusicSongTitle = "Surrounded";

        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<DynamoDbClient> _logger;

        public DynamoDbClient(ILogger<DynamoDbClient> logger, IDynamoDBContext dynamoDbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dynamoDbContext = dynamoDbContext ?? throw new ArgumentNullException(nameof(dynamoDbContext));
        }

        public Task<EmailCodeEntity> LoadEmailCode()
        {
            return _dynamoDbContext.LoadAsync<EmailCodeEntity>(EmailCodeId);
        }

        public Task<MusicDto> LoadMusicDto(CancellationToken token = default)
        {
            return _dynamoDbContext.LoadAsync<MusicDto>(MusicArtist, MusicSongTitle, token);
        }

        public async Task SaveRecord(DynamoDbCity obj)
        {
            const int maxAttempts = 3;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
                try
                {
                    await _dynamoDbContext.SaveAsync(obj);
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
