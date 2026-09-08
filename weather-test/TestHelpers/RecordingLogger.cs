using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace weather_test.TestHelpers
{
    /// <summary>
    /// Captures formatted log messages so tests can assert on what was, and was not, logged.
    /// </summary>
    internal sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = new();

        /// <summary>
        /// The named parameters of each record, before the message template is rendered. Asserting here
        /// rather than on <see cref="Messages"/> is what distinguishes a structured record from a string
        /// that merely happens to contain the value.
        /// </summary>
        public List<IReadOnlyList<KeyValuePair<string, object?>>> Entries { get; } = new();

        /// <summary>State of every scope opened, in the order they were opened.</summary>
        public List<object> Scopes { get; } = new();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            Scopes.Add(state);
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));

            if (state is IReadOnlyList<KeyValuePair<string, object?>> values)
            {
                Entries.Add(values);
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
