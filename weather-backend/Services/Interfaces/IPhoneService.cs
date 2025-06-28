using weather_backend.Models.PhoneService;

namespace weather_backend.Services.Interfaces
{
    public interface IPhoneService
    {
        ValidatePhoneNumberModel ValidatePhoneNumber(string phone);
    }
}
