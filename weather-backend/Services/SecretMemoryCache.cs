using Microsoft.Extensions.Caching.Memory;

namespace weather_backend.Services
{
    public class SecretMemoryCache
    {
        public MemoryCache Cache { get; } = new(
            new MemoryCacheOptions
            {
                SizeLimit = 1024
            });
    }
}