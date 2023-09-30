using Microsoft.AspNetCore.Mvc;
using weather_backend.Structs;

namespace weather_backend.Controllers
{
    [Route("api/struc")]
    [ApiController]
    public sealed class StrucTestController : ControllerBase
    {
        [HttpGet]
        [Route("nice")]
        public ActionResult<string> Get()
        {
            var structnew = new TestStruc() { Age = 11, Name = "haha nice" };
            modifythings(structnew);
            return Ok(structnew);
        }

        private void modifythings(TestStruc testStruc)
        {
            testStruc.Name = "gwagawgwa";
        }
    }
}
