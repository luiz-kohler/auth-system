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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _service.Login(request);
            return Ok(token);
        }

        [HttpGet]
        public async Task<IActionResult> GetMany([FromQuery] GetManyUsersRequest request)
        {
            var users = await _service.GetMany(request);
            return Ok(users);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _service.Delete(id);
            return Ok();
        }

    }
}
