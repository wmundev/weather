using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using weather_backend.Dto;
using weather_backend.Repository;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("api/email")]
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
        /// <returns>
        /// A <see cref="Task{EmailCodeEntity}"/> representing the asynchronous operation,
        /// containing the <see cref="EmailCodeEntity"/> retrieved from the database.
        /// </returns>
        [HttpGet]
        [Route("code")]
        public async Task<EmailCodeEntity> GetEmailCode()
        {
            return await _client.LoadEmailCode();
        }
    }
}
