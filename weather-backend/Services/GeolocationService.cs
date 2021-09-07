using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace weather_backend.Services
{
    public class GeolocationService
    {
        private HttpClient _httpClient;
        private IActionContextAccessor _accessor;

        public GeolocationService(HttpClient httpClient, IActionContextAccessor accessor)
        {
            _httpClient = httpClient;
            _accessor = accessor;
        }

        public async Task<string> GetIpAddress()
        {
            string clientIpAddress = _accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString();
            return clientIpAddress;
        }

        public async Task<string> GetLocation()
        {
            string clientIpAddress = await GetIpAddress();

            string baseUrl = "http://ip-api.com/json";
            string locationInfo = await _httpClient.GetStringAsync($"{baseUrl}/{clientIpAddress}");
            return locationInfo;
        }
    }
}