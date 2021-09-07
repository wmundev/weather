using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PhoneNumbers;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PhoneNumberController: ControllerBase
    {
        [HttpGet]
        [Route("/phone")]
        public async Task<string> GetCurrentWeatherDataById([FromQuery(Name = "phone")] string phone)
        {
            var phoneNumberUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
            var phoneNumber = phoneNumberUtil.Parse(phone, "AU");
            Console.WriteLine(phoneNumber.CountryCodeSource);
            Console.WriteLine(phoneNumberUtil.IsPossibleNumber(phoneNumber));
            Console.WriteLine(phoneNumberUtil.GetNumberType(phoneNumber));
            return "";
        }

    }
}