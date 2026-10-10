using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace weather_backend.ActionFilter
{
    /// <summary>
    /// Rejects a request unless its <c>X-Api-Key</c> header matches the key held in configuration.
    /// </summary>
    /// <remarks>
    /// Fails closed: when no key is configured the endpoint answers 503 rather than becoming public. In
    /// production the key arrives as an environment variable (<c>EmailCode__ApiKey</c> for
    /// <c>EmailCode:ApiKey</c>), set on the Beanstalk environment by the deployment project from a GitHub
    /// Actions secret.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequireApiKeyAttribute : Attribute, IAuthorizationFilter
    {
        public const string HeaderName = "X-Api-Key";

        public RequireApiKeyAttribute(string configurationKey)
        {
            ConfigurationKey = configurationKey;
        }

        /// <summary>The configuration key holding the expected API key, such as <c>EmailCode:ApiKey</c>.</summary>
        public string ConfigurationKey { get; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var expectedKey = configuration[ConfigurationKey];

            if (string.IsNullOrWhiteSpace(expectedKey))
            {
                context.Result = Problem(StatusCodes.Status503ServiceUnavailable, "Endpoint not configured.",
                    "This endpoint is disabled until its API key is configured.");
                return;
            }

            var suppliedKey = context.HttpContext.Request.Headers[HeaderName].ToString();
            if (!KeysMatch(suppliedKey, expectedKey))
            {
                context.Result = Problem(StatusCodes.Status401Unauthorized, "Invalid API key.",
                    $"Supply a valid key in the {HeaderName} header.");
            }
        }

        /// <summary>
        /// Constant-time comparison. Both sides are hashed first so the comparison also takes the same time
        /// regardless of how long the supplied key is.
        /// </summary>
        private static bool KeysMatch(string supplied, string expected)
        {
            return CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(Encoding.UTF8.GetBytes(supplied)),
                SHA256.HashData(Encoding.UTF8.GetBytes(expected)));
        }

        private static ObjectResult Problem(int status, string title, string detail)
        {
            return new ObjectResult(new ProblemDetails { Status = status, Title = title, Detail = detail }) { StatusCode = status };
        }
    }
}
