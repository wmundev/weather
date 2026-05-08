// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Logging;
using PhoneNumbers;
using Weather.CLI.Services;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger<SimpleService>();

// var simpleService = new SimpleService(logger);

// for (int i = 0; i < 1000000; i++)
// {
//     simpleService.DoThings();
// }

var phoneNumberUtil = PhoneNumberUtil.GetInstance();
var timeZonesMapper = PhoneNumberToTimeZonesMapper.GetInstance();
var carrierMapper = PhoneNumberToCarrierMapper.GetInstance();
var random = new Random();
int validCount = 0;
int invalidCount = 0;

Console.WriteLine("Testing 100 random phone numbers...");
Console.WriteLine($"{"Number",-15} | {"Valid",-5} | {"Type",-28} | {"Region",-6} | {"Carrier",-15} | {"Intl Format",-18} | {"TimeZones"}");
Console.WriteLine(new string('-', 120));

for (int i = 0; i < 20; i++)
{
    string[] prefixes = {"+919", "+918", "+4474", "+4475", "+4477", "+4478", "+4479", "+614"};
    string prefix = prefixes[random.Next(prefixes.Length)];
    string randomPhone = $"{prefix}{random.Next(10000000, 99999999)}";
    try
    {
        var numberInfo = phoneNumberUtil.Parse(randomPhone, "ZZ");
        bool isValid = phoneNumberUtil.IsValidNumber(numberInfo);

        if (isValid)
            validCount++;
        else
            invalidCount++;

        var numberType = phoneNumberUtil.GetNumberType(numberInfo).ToString();
        var regionCode = phoneNumberUtil.GetRegionCodeForNumber(numberInfo) ?? "";
        var timeZones = string.Join(", ", timeZonesMapper.GetTimeZonesForNumber(numberInfo));
        var carrier = carrierMapper.GetNameForNumber(numberInfo, Locale.English) ?? "";
        var intlFormat = phoneNumberUtil.Format(numberInfo, PhoneNumberFormat.INTERNATIONAL);

        Console.WriteLine($"{randomPhone,-15} | {isValid,-5} | {numberType,-28} | {regionCode,-6} | {carrier,-15} | {intlFormat,-18} | {timeZones}");
    }
    catch (NumberParseException ex)
    {
        Console.WriteLine($"{randomPhone,-15} | {"Error",-5} | {ex.Message}");
        invalidCount++;
    }
}

Console.WriteLine($"\nTest Complete. Valid: {validCount}, Invalid: {invalidCount}");
