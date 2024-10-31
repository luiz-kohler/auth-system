using Auth_API.DTOs;
using Auth_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth_API.Controllers
{
    [Route("endpoints")]
    public class EndpointsController : Controller
    {
        private readonly IEndpointService _service;

        public EndpointsController(IEndpointService service)
        {
            _service = service;
        }

        [HttpPost("projects/{projectId}")]
        public async Task<IActionResult> Create([FromBody] List<CreateEndpointRequest> request, int projectId)
        {
            await _service.Create(request, projectId);
            return Ok();
        }
    }
}
