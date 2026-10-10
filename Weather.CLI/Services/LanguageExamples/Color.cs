namespace Weather.CLI.Services.LanguageExamples
{
    /// <summary>
    /// The builder pattern on an immutable type. Moved from the API, where nothing used it.
    /// </summary>
    public sealed class Color(byte red, byte green, byte blue, byte alpha)
    {
        public byte Red { get; } = red;
        public byte Green { get; } = green;
        public byte Blue { get; } = blue;
        public byte Alpha { get; } = alpha;

        public sealed class Builder
        {
            private byte _alpha;
            private byte _blue;
            private byte _green;
            private byte _red;

            public Builder Red(byte red)
            {
                _red = red;
                return this;
            }

            public Builder Green(byte green)
            {
                _green = green;
                return this;
            }

            public Builder Blue(byte blue)
            {
                _blue = blue;
                return this;
            }

            public Builder Alpha(byte alpha)
            {
                _alpha = alpha;
                return this;
            }

            public Color Create()
            {
                return new Color(_red, _green, _blue, _alpha);
            }
        }
    }
}
