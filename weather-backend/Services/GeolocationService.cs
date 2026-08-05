using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace weather_backend.Services
{
    public class GeolocationService : IGeolocationService
    {
        private const string BaseUrl = "http://ip-api.com/json";
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GeolocationService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public Task<string> GetIpAddress()
        {
            var clientIpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            return Task.FromResult(clientIpAddress ?? "");
        }

        /// <summary>
        /// Looks up the caller's location, or returns null when the remote IP address is unavailable
        /// (for example when the request did not arrive over a socket).
        /// </summary>
        public async Task<string?> GetLocation()
        {
            var clientIpAddress = await GetIpAddress();
            if (string.IsNullOrEmpty(clientIpAddress))
            {
                return null;
            }

            return await _httpClient.GetStringAsync($"{BaseUrl}/{clientIpAddress}");
        }
    }
}
