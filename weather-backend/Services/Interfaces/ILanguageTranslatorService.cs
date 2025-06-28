using System.Threading.Tasks;

namespace weather_backend.Services.Interfaces
{
    public interface ILanguageTranslatorService
    {
        Task<string> GenerateLanguageFileAsync();
    }
}
