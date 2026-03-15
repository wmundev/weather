using System;

namespace weather_backend.Exceptions
{
    /// <summary>
    /// Base exception for weather service related errors
    /// </summary>
    public class WeatherServiceException : Exception
    {
        public WeatherServiceException()
        {
        }

        public WeatherServiceException(string message)
            : base(message)
        {
        }

        public WeatherServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when configuration is missing or invalid
    /// </summary>
    public class ConfigurationException : WeatherServiceException
    {
        public ConfigurationException(string message)
            : base(message)
        {
        }

        public ConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when secret retrieval fails
    /// </summary>
    public class SecretNotFoundException : WeatherServiceException
    {
        public string SecretKey { get; }

        public SecretNotFoundException(string secretKey)
            : base($"Secret not found: {secretKey}")
        {
            SecretKey = secretKey;
        }

        public SecretNotFoundException(string secretKey, Exception innerException)
            : base($"Secret not found: {secretKey}", innerException)
        {
            SecretKey = secretKey;
        }
    }

    /// <summary>
    /// Exception thrown when AWS credentials cannot be obtained
    /// </summary>
    public class CredentialsNotAvailableException : WeatherServiceException
    {
        public CredentialsNotAvailableException(string message)
            : base(message)
        {
        }

        public CredentialsNotAvailableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
