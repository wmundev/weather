﻿using System.Collections.Immutable;
using System.Text.Json;

namespace weather_backend
{
    public static class Constants
    {
        public const string SECRETS_KEY = "weather_secrets";

        public const string CRON_EXPRESSION_SCHEDULE_JOB = "0 22 * * *";

        public static JsonSerializerOptions DefaultJsonOptions => new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true};

        public static JsonSerializerOptions CamelCaseJsonOptions => new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

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
