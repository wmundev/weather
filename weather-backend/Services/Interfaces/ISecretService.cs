using System.Threading.Tasks;

namespace weather_backend.Services.Interfaces
{
    public interface ISecretService
    {
        public Task<string> FetchSecret(string secretKey);
        public Task<string?> FetchSpecificSecret(string secretKey);
    }
}
