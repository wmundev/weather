using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using weather_backend.ActionFilter;
using weather_backend.Dto;
using weather_backend.Repository;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("api/email")]
    [ProducesResponseType(typeof(ProblemDetails), 429)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public class EmailController : ControllerBase
    {
        private readonly IDynamoDbClient _client;

        public EmailController(IDynamoDbClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        /// Retrieves the email code entity from the database.
        /// </summary>
        /// <remarks>
        /// Requires the <c>X-Api-Key</c> header: the entity carries a code and an email address, so it must not
        /// be readable by anyone who finds the URL.
        /// </remarks>
        /// <returns>The <see cref="EmailCodeEntity"/> retrieved from the database.</returns>
        /// <response code="200">Returns the stored email code.</response>
        /// <response code="401">If the X-Api-Key header is missing or wrong.</response>
        /// <response code="503">If no API key is configured for this endpoint.</response>
        [HttpGet]
        [Route("code")]
        [RequireApiKey("EmailCode:ApiKey")]
        [ProducesResponseType(typeof(EmailCodeEntity), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<EmailCodeEntity>> GetEmailCode()
        {
            return await _client.LoadEmailCode();
        }
    }
}
