using weather_backend.Models;

namespace weather_backend.Services.Interfaces
{
    public interface IAcademicService
    {
        Academic? GetAcademicById(int id);
    }
}
