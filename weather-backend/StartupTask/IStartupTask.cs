using System.Threading;
using System.Threading.Tasks;

namespace weather_backend.StartupTask
{
    public interface IStartupTask
    {
        Task ExecuteAsync(CancellationToken cancellationToken = default);
    }
}