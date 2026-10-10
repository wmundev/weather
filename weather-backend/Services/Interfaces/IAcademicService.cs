using System.Threading;
using System.Threading.Tasks;
using weather_backend.Models;

namespace weather_backend.Services.Interfaces
{
    public interface IAcademicService
    {
        /// <summary>Returns the academic with the given ACNUM, or null when there is none.</summary>
        Task<Academic?> GetAcademicByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
