using PhoneNumbers;

namespace weather_backend.Services
{
    public interface IPhoneService
    {
        dynamic ValidatePhoneNumber(string phone);
    }

    public class PhoneService : IPhoneService
    {
        private readonly PhoneNumberUtil _phoneNumberUtil;

        public PhoneService()
        {
            _phoneNumberUtil = PhoneNumberUtil.GetInstance();
        }

        public dynamic ValidatePhoneNumber(string phone)
        {
            var phoneNumberUtil = _phoneNumberUtil;
            var phoneNumber = phoneNumberUtil.Parse(phone, "AU");

            var countryCode = phoneNumber.CountryCodeSource;
            var possibleNumber = phoneNumberUtil.IsPossibleNumber(phoneNumber);
            var numberType = phoneNumberUtil.GetNumberType(phoneNumber);
            return new {countryCode, possibleNumber, numberType};
        }
    }
}