using System.Text.Json;

namespace Weather.API.IntegrationTests
{
    public static class Constants
    {
        public static JsonSerializerOptions CamelCaseJsonOptions => new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
    }
}
