﻿using Auth_Background_Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth_API.Controllers
{
    [Route("auth-tests")]
    public class AuthTestsController : ControllerBase
    {
        [Public]
        [HttpGet("no-auth-needed")]
        public ActionResult NoAuthNeeded()
        {
            return Ok("^_~");
        }

        [HttpGet("auth-needed")]
        public ActionResult AuthNeeded()
        {
            return Ok("^_~");
        }
    }
}
