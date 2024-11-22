using Auth_API.DTOs;
using Auth_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth_API.Controllers
{
    [Route("roles")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _service;

        public RolesController(IRoleService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
        {
            await _service.Create(request);
            return Ok();
        }

        [HttpPost("delete-many")]
        public async Task<IActionResult> Delete([FromBody] DeleteRolesRequest request)
        {
            await _service.Delete(request.Ids);
            return Ok();
        }

        [HttpPost("link-endpoints")]
        public async Task<IActionResult> LinkEndpoints([FromBody] LinkToEndpoint request)
        {
            await _service.AddEndpoints(request.Id, request.Endpoints);
            return Ok();
        }

        [HttpPost("unlink-endpoints")]
        public async Task<IActionResult> UnlinkEndpoints([FromBody] UnlinkToEndpoint request)
        {
            await _service.RemoveEndpoints(request.Id, request.Endpoints);
            return Ok();
        }

        [HttpPost("search-many")]
        public async Task<IActionResult> GetMany([FromBody] GetManyRolesRequest request)
        {
            var response = await _service.GetMany(request);
            return Ok(response);
        }

        [HttpPost("search-one")]
        public async Task<IActionResult> Get([FromBody] GetOneRoleRequest request)
        {
            var response = await _service.Get(request.Id);
            return Ok(response);
        }

        [HttpPost("link-users")]
        public async Task<IActionResult> LinkToUsers([FromBody] LinkUsersRequest request)
        {
            await _service.LinkUsers(request.UserIds, request.RoleIds);
            return Ok();
        }

        [HttpPost("unlink-users")]
        public async Task<IActionResult> RemoveFromRoles([FromBody] UnlinkUsersRequest request)
        {
            await _service.UnlinkUsers(request.UserIds, request.RoleIds);
            return Ok();
        }
    }
}
