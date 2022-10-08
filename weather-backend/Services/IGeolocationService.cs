using System.Threading.Tasks;

namespace weather_backend.Services
{
    public interface IGeolocationService
    {
        Task<string> GetIpAddress();
        Task<string> GetLocation();
    }
}