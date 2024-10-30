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

        [HttpPost("{id}/add-to-projects")]
        public async Task<IActionResult> AddToProjects([FromRoute] int id, [FromBody] List<int> projectIds)
        {
            await _service.AddToProjects(id, projectIds);
            return Ok();
        }

        [HttpDelete("{id}/remove-from-projects")]
        public async Task<IActionResult> RemoveFromProjects([FromRoute] int id, [FromBody] List<int> projectIds)
        {
            await _service.RemoveFromProjects(id, projectIds);
            return Ok();
        }

        [HttpPost("{id}/add-to-roles")]
        public async Task<IActionResult> AddToRoles([FromRoute] int id, [FromBody] List<int> roleIds)
        {
            await _service.AddToRoles(id, roleIds);
            return Ok();
        }

        [HttpDelete("{id}/remove-from-roles")]
        public async Task<IActionResult> RemoveFromRoles([FromRoute] int id, [FromBody] List<int> roleIds)
        {
            await _service.RemoveFromRoles(id, roleIds);
            return Ok();
        }
    }
}
