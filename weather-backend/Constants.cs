using System.Collections.Immutable;
using System.Text.Json;

namespace weather_backend
{
    public static class Constants
    {
        public const string SECRETS_KEY = "weather_secrets";

        public const string CRON_EXPRESSION_SCHEDULE_JOB = "0 22 * * *";

        /// <summary>
        /// OpenWeatherMap city id for Melbourne, used by the daily digest and the legacy /weather endpoint.
        /// </summary>
        public const double DEFAULT_CITY_ID = 7839805;

        // Cached instances, not properties: System.Text.Json builds and caches serialization metadata per
        // options object, so handing out a fresh instance on every access throws that cache away.
        public static readonly JsonSerializerOptions DefaultJsonOptions = new() {PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true};

        public static readonly JsonSerializerOptions CamelCaseJsonOptions = new() {PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

        /**
         * See https://docs.aws.amazon.com/translate/latest/dg/what-is-languages.html
         */
        public static readonly ImmutableList<string> LANGUAGE_CODE = ImmutableList.Create(
            "af",
            "sq",
            "am",
            "ar",
            "hy",
            "az",
            "bn",
            "bs",
            "bg",
            "ca",
            "zh",
            "zh-TW",
            "hr",
            "cs",
            "da",
            "fa-AF",
            "nl",
            "en",
            "et",
            "fa",
            "tl",
            "fi",
            "fr",
            "fr-CA",
            "ka",
            "de",
            "el",
            "gu",
            "ht",
            "ha",
            "he",
            "hi",
            "hu",
            "is",
            "id",
            "ga",
            "it",
            "ja",
            "kn",
            "kk",
            "ko",
            "lv",
            "lt",
            "mk",
            "ms",
            "ml",
            "mt",
            "mr",
            "mn",
            "no",
            "ps",
            "pl",
            "pt",
            "pt-PT",
            "pa",
            "ro",
            "ru",
            "sr",
            "si",
            "sk",
            "sl",
            "so",
            "es",
            "es-MX",
            "sw",
            "sv",
            "ta",
            "te",
            "th",
            "tr",
            "uk",
            "ur",
            "uz",
            "vi",
            "cy");
    }
}
