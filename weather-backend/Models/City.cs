using Newtonsoft.Json;

namespace weather_backend.Models
{
    public class City
    {
        public City()
        {
        }

        public City(double id, string name, string state, string country, Coordinate coordinate)
        {
            Id = id;
            Name = name;
            State = state;
            Country = country;
            Coordinate = coordinate;
        }

        [JsonProperty("id")] public double Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("state")] public string State { get; set; }
        [JsonProperty("country")] public string Country { get; set; }
        [JsonProperty("coord")] public Coordinate Coordinate { get; set; }
    }
}