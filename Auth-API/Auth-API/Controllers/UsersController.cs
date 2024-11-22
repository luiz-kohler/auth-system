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

        [HttpPost("search-many")]
        public async Task<IActionResult> GetMany([FromBody] GetManyUsersRequest request)
        {
            var users = await _service.GetMany(request);
            return Ok(users);
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteUserRequest request)
        {
            await _service.Delete(request.Id);
            return Ok();
        }

        [HttpPost("link-to-project")]
        public async Task<IActionResult> AddToProjects([FromBody] LinkToProjectsRequest request)
        {
            await _service.LinkToProjects(request.Id, request.ProjectIds);
            return Ok();
        }

        [HttpPost("unlink-from-project")]
        public async Task<IActionResult> RemoveFromProjects([FromBody] UnlinkFromProjectsRequest request)
        {
            await _service.UnlinkFromProjects(request.Id, request.ProjectIds);
            return Ok();
        }

        [Public]
        [HttpPost("has-access-to-endpoint")]
        public async Task<IActionResult> VerifyUserHasAccess([FromBody] VerifyUserHasAccessRequest request)
        {
            var response = await _service.VerifyUserHasAccess(request.EndpointId);
            return Ok(response);
        }

        [Public]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var response = await _service.RefreshToken(request);
            return Ok(response);
        }
    }
}
