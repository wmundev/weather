namespace weather_backend.Configuration
{
    /// <summary>
    /// Rate limiting knobs, bound from the "RateLimiting" configuration section so the limits can be
    /// tuned per environment without a redeploy of new code.
    /// </summary>
    public class RateLimitingOptions
    {
        public const string SectionName = "RateLimiting";

        /// <summary>Requests allowed per window for a single caller across the whole API.</summary>
        public int PermitLimit { get; set; } = 100;

        /// <summary>Length of the global window, in seconds.</summary>
        public int WindowSeconds { get; set; } = 60;

        /// <summary>
        /// Requests allowed per window for the uncached endpoints. These reach OpenWeatherMap on every
        /// call rather than serving from the one-hour cache, so they spend third-party quota directly.
        /// </summary>
        public int UncachedPermitLimit { get; set; } = 10;

        /// <summary>Length of the uncached-endpoint window, in seconds.</summary>
        public int UncachedWindowSeconds { get; set; } = 60;

        /// <summary>
        /// Number of windows a caller's counter is subdivided into. Higher means a smoother limit and a
        /// smaller burst at a window boundary, at the cost of a little more bookkeeping per caller.
        /// </summary>
        public int SegmentsPerWindow { get; set; } = 4;
    }
}
