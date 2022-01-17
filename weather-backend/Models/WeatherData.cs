using Newtonsoft.Json;

namespace weather_backend.Models;

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
    [JsonProperty("coord")] public Coordinate coord { get; set; }

    [JsonProperty("weather")] public Weather[] weather { get; set; }

    [JsonProperty("base")] public string BaseInfo { get; set; }

    [JsonProperty("main")] public MainWeather main { get; set; }

    [JsonProperty("visibility")] public int visibility { get; set; }

    [JsonProperty("wind")] public Wind wind { get; set; }

    [JsonProperty("clouds")] public Cloud clouds { get; set; }

    [JsonProperty("dt")] public int dt { get; set; }

    [JsonProperty("sys")] public WeatherSystem sys { get; set; }

    [JsonProperty("timezone")] public int timezone { get; set; }

    [JsonProperty("id")] public int id { get; set; }

    [JsonProperty("name")] public string name { get; set; }

    [JsonProperty("cod")] public int cod { get; set; }
}