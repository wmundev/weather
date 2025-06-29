﻿using PhoneNumbers;
using weather_backend.Models.PhoneService;
using weather_backend.Services.Interfaces;

namespace weather_backend.Services
{
    public class PhoneService : IPhoneService
    {
        private readonly PhoneNumberUtil _phoneNumberUtil;

        public PhoneService()
        {
            _phoneNumberUtil = PhoneNumberUtil.GetInstance();
        }

        public ValidatePhoneNumberModel ValidatePhoneNumber(string phone)
        {
            var phoneNumberUtil = _phoneNumberUtil;
            var phoneNumber = phoneNumberUtil.Parse(phone, "AU");

            var countryCode = phoneNumber.CountryCodeSource;
            var possibleNumber = phoneNumberUtil.IsPossibleNumber(phoneNumber);
            var numberType = phoneNumberUtil.GetNumberType(phoneNumber);
            return new ValidatePhoneNumberModel {CountryCode = countryCode, PossibleNumber = possibleNumber, NumberType = numberType};
        }
    }
}
