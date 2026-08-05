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
        private readonly SecretMemoryCache _memoryCache;
        private readonly IAmazonSimpleSystemsManagement _ssmClient;
        private readonly ILogger<SecretService> _logger;

        public SecretService(IAmazonSimpleSystemsManagement ssmClient, SecretMemoryCache memoryCache, ILogger<SecretService> logger)
        {
            _ssmClient = ssmClient ?? throw new ArgumentNullException(nameof(ssmClient));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string?> FetchSpecificSecret(string secretKey)
        {
            var allSecrets = await FetchSecret(Constants.SECRETS_KEY);
            var deserialisedObject = JsonSerializer.Deserialize<AllSecrets>(allSecrets);
            return typeof(AllSecrets).GetProperty(secretKey)?.GetValue(deserialisedObject)?.ToString();
        }


        public async Task<string> FetchSecret(string secretKey)
        {
            var cacheKey = $"secret_{secretKey}";

            if (!_memoryCache.Cache.TryGetValue(cacheKey, out string? cacheValue))
            {
                _logger.LogDebug("Secret cache miss for key: {SecretKey}", secretKey);
                var parameterStoreValue = await FetchSecretFromParameterStore(secretKey);

                cacheValue = parameterStoreValue?.Parameter?.Value;

                // cache secret for 24 hours
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(24))
                    .SetSize(1);

                _memoryCache.Cache.Set(cacheKey, cacheValue, cacheEntryOptions);
            }
            else
            {
                _logger.LogDebug("Secret cache hit for key: {SecretKey}", secretKey);
            }

            return cacheValue ?? "{}";
        }

        private async Task<GetParameterResponse> FetchSecretFromParameterStore(string secretKey)
        {
            var getParameterRequest = new GetParameterRequest {Name = secretKey, WithDecryption = true};
            var secret = await _ssmClient.GetParameterAsync(getParameterRequest);
            return secret;
        }
    }
}
