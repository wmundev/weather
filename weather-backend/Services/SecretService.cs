using System;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Caching.Memory;
using weather_backend.Models;

namespace weather_backend.Services
{
    public interface ISecretService
    {
        public Task<string> FetchSecret(string secretKey);
        public Task<string?> FetchSpecificSecret(string secretKey);
    }

    public class SecretService : ISecretService
    {
        private readonly SecretMemoryCache _memoryCache;
        private readonly IAmazonSimpleSystemsManagement _ssmClient;

        public SecretService(IAmazonSimpleSystemsManagement ssmClient, SecretMemoryCache memoryCache)
        {
            _ssmClient = ssmClient;
            _memoryCache = memoryCache;
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
                Console.WriteLine("not using cache");
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
                Console.WriteLine("using cache");
            }

            return cacheValue ?? "{}";
        }

        private async Task<GetParameterResponse> FetchSecretFromParameterStore(string secretKey)
        {
            var getParameterRequest = new GetParameterRequest { Name = secretKey, WithDecryption = true };
            var secret = await _ssmClient.GetParameterAsync(getParameterRequest);
            return secret;
        }
    }
}
