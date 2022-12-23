namespace weather_backend.Models
{
    public class AllSecrets
    {
        public required string OpenWeatherApiKey { get; set; }
        public required string SMTPUsername { get; set; }
        public required string SMTPPassword { get; set; }
    }
}
