using System;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using weather_backend.Models;
using weather_backend.Services.Interfaces;

namespace weather_backend.Services
{
    public sealed class SecretService : ISecretService
    {
        private readonly ILogger<SecretService> _logger;
        private readonly SecretMemoryCache _memoryCache;
        private readonly IAmazonSimpleSystemsManagement _ssmClient;

        public SecretService(IAmazonSimpleSystemsManagement ssmClient, SecretMemoryCache memoryCache, ILogger<SecretService> logger)
        {
            _ssmClient = ssmClient ?? throw new ArgumentNullException(nameof(ssmClient));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string?> FetchSpecificSecret(string secretKey)
        {
            var allSecrets = await FetchSecret(Constants.SECRETS_KEY);

            AllSecrets? deserialisedObject;
            try
            {
                deserialisedObject = JsonSerializer.Deserialize<AllSecrets>(allSecrets);
            }
            catch (JsonException ex)
            {
                // Every member of AllSecrets is required, so a partial or malformed document throws here.
                // Surface it as a configuration problem rather than a bare JSON error.
                throw new InvalidOperationException(
                    $"SSM parameter '{Constants.SECRETS_KEY}' does not contain a valid {nameof(AllSecrets)} document.", ex);
            }

            return typeof(AllSecrets).GetProperty(secretKey)?.GetValue(deserialisedObject)?.ToString();
        }


        public async Task<string> FetchSecret(string secretKey)
        {
            var cacheKey = $"secret_{secretKey}";

            if (_memoryCache.Cache.TryGetValue(cacheKey, out string? cacheValue) && cacheValue is not null)
            {
                _logger.LogDebug("Secret {SecretKey} served from cache", secretKey);
                return cacheValue;
            }

            _logger.LogDebug("Secret {SecretKey} not cached, fetching from Parameter Store", secretKey);
            var parameterStoreValue = await FetchSecretFromParameterStore(secretKey);
            var fetchedValue = parameterStoreValue?.Parameter?.Value;

            // An empty parameter is a deployment problem. Caching it would repeat the failure for a
            // full day and surface it far away from the cause, so fail fast instead.
            if (string.IsNullOrEmpty(fetchedValue))
            {
                throw new InvalidOperationException($"SSM parameter '{secretKey}' has no value.");
            }

            // cache secret for 24 hours
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(24))
                .SetSize(1);

            _memoryCache.Cache.Set(cacheKey, fetchedValue, cacheEntryOptions);

            return fetchedValue;
        }

        private async Task<GetParameterResponse> FetchSecretFromParameterStore(string secretKey)
        {
            var getParameterRequest = new GetParameterRequest {Name = secretKey, WithDecryption = true};
            var secret = await _ssmClient.GetParameterAsync(getParameterRequest);
            return secret;
        }
    }
}
