using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace weather_backend.Services
{
    public class GeolocationService : IGeolocationService
    {
        private readonly IActionContextAccessor _accessor;
        private readonly HttpClient _httpClient;

        public GeolocationService(HttpClient httpClient, IActionContextAccessor accessor)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        public Task<string> GetIpAddress()
        {
            var clientIpAddress = _accessor.ActionContext?.HttpContext.Connection.RemoteIpAddress?.ToString();
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