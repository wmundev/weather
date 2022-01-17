﻿using Newtonsoft.Json;

namespace weather_backend.Models;

public class Weather
{
    [JsonProperty("id")] public int id { get; set; }

    [JsonProperty("main")] public string main { get; set; }

    [JsonProperty("description")] public string description { get; set; }

    [JsonProperty("icon")] public string icon { get; set; }
}