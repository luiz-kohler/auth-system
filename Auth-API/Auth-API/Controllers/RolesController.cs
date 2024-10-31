using Auth_API.DTOs;
using Auth_API.Services;
using Auth_API.Validator;
using Microsoft.AspNetCore.Mvc;

namespace Auth_API.Controllers
{
    [Route("roles")]
    public class RolesController : Controller
    {
        private readonly IRoleService _service;

        public RolesController(IRoleService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
        {
            await _service.Create(request);
            return Ok();
        }
    }
}
