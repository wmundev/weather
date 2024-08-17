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

        [HttpGet]
        [Route("code")]
        public async Task<EmailCodeEntity> GetEmailCode()
        {
            return await _client.LoadEmailCode();
        }
    }
}
