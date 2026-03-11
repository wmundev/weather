using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace weather_backend.Services
{
    public class GeolocationService : IGeolocationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;

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

        public async Task<string> GetLocation()
        {
            var clientIpAddress = await GetIpAddress();

            Console.WriteLine(isLocalIpAddress(clientIpAddress));

            var baseUrl = "http://ip-api.com/json";
            var locationInfo = await _httpClient.GetStringAsync($"{baseUrl}/{clientIpAddress}");
            return locationInfo;
        }

        private bool isLocalIpAddress(string ipAddress)
        {
            return IPAddress.Parse(ipAddress).AddressFamily == AddressFamily.InterNetwork;
        }
    }
}
