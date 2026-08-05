using System.Threading.Tasks;

namespace weather_backend.Services
{
    public interface IGeolocationService
    {
        Task<string> GetIpAddress();

        /// <summary>
        /// Returns the caller's location, or null when the remote IP address is unavailable.
        /// </summary>
        Task<string?> GetLocation();
    }
}