using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace weather_backend.Services
{
    public class GeolocationService
    {
        private readonly IActionContextAccessor _accessor;
        private readonly HttpClient _httpClient;

        public GeolocationService(HttpClient httpClient, IActionContextAccessor accessor)
        {
            _httpClient = httpClient;
            _accessor = accessor;
        }

        public async Task<string> GetIpAddress()
        {
            var clientIpAddress = _accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString();
            return clientIpAddress;
        }

        public async Task<string> GetLocation()
        {
            var clientIpAddress = await GetIpAddress();

            var baseUrl = "http://ip-api.com/json";
            var locationInfo = await _httpClient.GetStringAsync($"{baseUrl}/{clientIpAddress}");
            return locationInfo;
        }
    }
}