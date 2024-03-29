﻿namespace weather_backend.Factory
{
    public class Color
    {
        public Color(byte red, byte green, byte blue, byte alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public byte Red { get; }
        public byte Green { get; }
        public byte Blue { get; }
        public byte Alpha { get; }

        public class Builder
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