namespace weather_backend.Dto
{
    public enum WeatherUnit
    {
        Standard, // Kelvin
        Metric, // Celsius
        Imperial // Fahrenheit
    }

    public class WeatherRequestDto
    {
        public WeatherUnit Units { get; set; } = WeatherUnit.Metric;
        public string? Language { get; set; }
    }

    public class CoordinatesWeatherRequestDto : WeatherRequestDto
    {
        public required double Latitude { get; set; }
        public required double Longitude { get; set; }
    }

    public class CityNameWeatherRequestDto : WeatherRequestDto
    {
        public required string CityName { get; set; }
        public string? StateCode { get; set; }
        public string? CountryCode { get; set; }
    }

    public class CityIdWeatherRequestDto : WeatherRequestDto
    {
        public required double CityId { get; set; }
    }

    public class ZipCodeWeatherRequestDto : WeatherRequestDto
    {
        public required string ZipCode { get; set; }
        public string CountryCode { get; set; } = "us";
    }
}
