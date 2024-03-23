using PhoneNumbers;

namespace weather_backend.Models.PhoneService
{
    public class ValidatePhoneNumberModel
    {
        public PhoneNumber.Types.CountryCodeSource CountryCode { get; init; }
        public bool PossibleNumber { get; init; }
        public PhoneNumberType NumberType { get; init; }
    }
}
