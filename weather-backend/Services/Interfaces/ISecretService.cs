using System.Threading.Tasks;
using weather_backend.Models;

namespace weather_backend.Services.Interfaces
{
    public interface ISecretService
    {
        /// <summary>
        /// Returns the raw value of a Parameter Store parameter, cached for 24 hours.
        /// </summary>
        public Task<string> FetchSecret(string secretKey);

        /// <summary>
        /// Returns the application secrets document, parsed once and cached for 24 hours.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The parameter is empty or not a valid secrets document.</exception>
        public Task<AllSecrets> GetSecrets();
    }
}
