using Amazon.DynamoDBv2.DataModel;

namespace weather_domain.DatabaseEntities
{
    /// <summary>
    /// DynamoDB entity for caching weather data with 1-hour TTL
    /// Uses the "weather" table with "name" as the hash key (reusing existing table structure)
    /// </summary>
    [DynamoDBTable("weather")]
    public class WeatherCache
    {
        /// <summary>
        /// Cache key - combination of query type and parameters
        /// Format: "coordinates:lat:lon:units:lang" or "cityname:name:state:country:units:lang"
        /// Maps to the "name" attribute (hash key) in the weather table
        /// </summary>
        [DynamoDBHashKey]
        [DynamoDBProperty("name")]
        public required string CacheKey { get; set; }

        /// <summary>
        /// Query type (coordinates, cityname, cityid, zipcode)
        /// </summary>
        [DynamoDBProperty("queryType")]
        public required string QueryType { get; set; }

        /// <summary>
        /// Serialized weather data (JSON)
        /// </summary>
        [DynamoDBProperty("weatherData")]
        public required string WeatherDataJson { get; set; }

        /// <summary>
        /// Timestamp when the cache entry was created (Unix timestamp)
        /// </summary>
        [DynamoDBProperty("createdAt")]
        public long CreatedAt { get; set; }

        /// <summary>
        /// TTL attribute - DynamoDB will automatically delete expired items
        /// Set to 1 hour (3600 seconds) from creation time
        /// </summary>
        [DynamoDBProperty("ttl")]
        public long TTL { get; set; }

        /// <summary>
        /// Optional: Store individual query parameters for reporting/debugging
        /// </summary>
        [DynamoDBProperty("parameters")]
        public string? Parameters { get; set; }
    }
}
