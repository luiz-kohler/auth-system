using Auth_API.DTOs;
using Auth_API.Services;
using Auth_Background_Service;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;

namespace Auth_API.Controllers
{
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [Public]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            var token = await _service.Create(request);
            return Ok(token);
        }

        [Public]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _service.Login(request);
            return Ok(token);
        }

        [Public]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var response = await _service.RefreshToken(request);
            return Ok(response);
        }

        [HttpPost("search-many")]
        public async Task<IActionResult> GetMany([FromBody] GetManyUsersRequest request)
        {
            var users = await _service.GetMany(request);
            return Ok(users);
        }

        [Public]
        [HttpPost("has-access-to-endpoint")]
        public async Task<IActionResult> VerifyUserHasAccess([FromBody] VerifyUserHasAccessRequest request)
        {
            var response = await _service.VerifyUserHasAccess(request.EndpointId);
            return Ok(response);
        }
    }
}
