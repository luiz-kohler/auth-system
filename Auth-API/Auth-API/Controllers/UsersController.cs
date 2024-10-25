using Auth_API.DTOs;
using Auth_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth_API.Controllers
{
    [Route("users")]
    public class UsersController : Controller
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            var token = await _service.Create(request);
            return Ok(token);
        }
    }
}
