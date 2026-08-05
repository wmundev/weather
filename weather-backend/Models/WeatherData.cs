using System.Text.Json.Serialization;
using weather_domain.Entities;

namespace weather_backend.Models
{
    /**
     * * Example
     * * {
     * "coord":{
     * "lon":144.9442,
     * "lat":-37.8131
     * },
     * "weather":[
     * {
     * "id":803,
     * "main":"Clouds",
     * "description":"broken clouds",
     * "icon":"04n"
     * }
     * ],
     * "base":"stations",
     * "main":{
     * "temp":290.78,
     * "feels_like":287.93,
     * "temp_min":289.82,
     * "temp_max":291.48,
     * "pressure":1011,
     * "humidity":77
     * },
     * "visibility":10000,
     * "wind":{
     * "speed":5.66,
     * "deg":180
     * },
     * "clouds":{
     * "all":75
     * },
     * "dt":1613905075,
     * "sys":{
     * "type":1,
     * "id":9548,
     * "country":"AU",
     * "sunrise":1613850973,
     * "sunset":1613898712
     * },
     * "timezone":39600,
     * "id":7839805,
     * "name":"Melbourne",
     * "cod":200
     * }
     */
    public class WeatherData
    {
        [JsonPropertyName("coord")] public required Coordinate coord { get; set; }

        [JsonPropertyName("weather")] public required Weather[] weather { get; set; }

        [JsonPropertyName("base")] public string? BaseInfo { get; set; }

        [JsonPropertyName("main")] public required MainWeather main { get; set; }

        [JsonPropertyName("visibility")] public int visibility { get; set; }

        [JsonPropertyName("wind")] public required Wind wind { get; set; }

        [JsonPropertyName("clouds")] public required Cloud clouds { get; set; }

        [JsonPropertyName("rain")] public Rain? rain { get; set; }

        [JsonPropertyName("snow")] public Snow? snow { get; set; }

        [JsonPropertyName("dt")] public int dt { get; set; }

        [JsonPropertyName("sys")] public required WeatherSystem sys { get; set; }

        [JsonPropertyName("timezone")] public int timezone { get; set; }

        [JsonPropertyName("id")] public int id { get; set; }

        [JsonPropertyName("name")] public required string name { get; set; }

        [JsonPropertyName("cod")] public int cod { get; set; }
    }
}
