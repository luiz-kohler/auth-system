using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth_API.Controllers
{
    [Route("auth-tests")]
    public class AuthTestsController : Controller
    {

        [HttpGet("no-auth-needed")]
        public ActionResult NoAuthNeeded()
        {
            return Ok("^_~");
        }

        [Authorize]
        [HttpGet("auth-needed")]
        public ActionResult AuthNeeded()
        {
            return Ok("^_~");
        }
    }
}
