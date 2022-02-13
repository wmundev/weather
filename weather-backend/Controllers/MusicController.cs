using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using weather_backend.Repository;

namespace weather_backend.Controllers;

[ApiController]
[Route("[controller]")]
public class MusicController : ControllerBase
{
    private readonly DynamoDbClient _client;

    public MusicController(DynamoDbClient client)
    {
        _client = client;
    }

    [HttpGet]
    [Route("music")]
    public async Task<string> GetIpAddress()
    {
        return (await _client.getthings()).SongTitle;
    }
}