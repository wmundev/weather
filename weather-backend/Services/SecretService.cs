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
        private const string SecretsCacheKey = "secrets:parsed";

        private readonly ILogger<SecretService> _logger;
        private readonly SecretMemoryCache _memoryCache;
        private readonly IAmazonSimpleSystemsManagement _ssmClient;

        public SecretService(IAmazonSimpleSystemsManagement ssmClient, SecretMemoryCache memoryCache, ILogger<SecretService> logger)
        {
            _ssmClient = ssmClient ?? throw new ArgumentNullException(nameof(ssmClient));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AllSecrets> GetSecrets()
        {
            // The parsed document is cached, not just the raw string: every weather request reads the API
            // key, and deserialising the JSON on each of them was wasted work.
            if (_memoryCache.Cache.TryGetValue(SecretsCacheKey, out AllSecrets? cachedSecrets) && cachedSecrets is not null)
            {
                return cachedSecrets;
            }

            var document = await FetchSecret(Constants.SECRETS_KEY);

            AllSecrets? secrets;
            try
            {
                secrets = JsonSerializer.Deserialize<AllSecrets>(document);
            }
            catch (JsonException ex)
            {
                // Every member of AllSecrets is required, so a partial or malformed document throws here.
                // Surface it as a configuration problem rather than a bare JSON error.
                throw InvalidSecretsDocument(ex);
            }

            if (secrets is null)
            {
                throw InvalidSecretsDocument(null);
            }

            _memoryCache.Cache.Set(SecretsCacheKey, secrets, CacheEntryOptions());

            return secrets;
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

            _memoryCache.Cache.Set(cacheKey, fetchedValue, CacheEntryOptions());

            return fetchedValue;
        }

        private static MemoryCacheEntryOptions CacheEntryOptions()
        {
            return new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(24))
                .SetSize(1);
        }

        private static InvalidOperationException InvalidSecretsDocument(Exception? innerException)
        {
            return new InvalidOperationException(
                $"SSM parameter '{Constants.SECRETS_KEY}' does not contain a valid {nameof(AllSecrets)} document.", innerException);
        }

        private async Task<GetParameterResponse> FetchSecretFromParameterStore(string secretKey)
        {
            var getParameterRequest = new GetParameterRequest {Name = secretKey, WithDecryption = true};
            var secret = await _ssmClient.GetParameterAsync(getParameterRequest);
            return secret;
        }
    }
}
