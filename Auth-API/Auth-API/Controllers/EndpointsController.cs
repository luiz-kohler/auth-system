using Auth_API.DTOs;
using Auth_API.Services;
using Auth_Background_Service;
using Microsoft.AspNetCore.Mvc;

namespace Auth_API.Controllers
{
    [Route("endpoints")]
    public class EndpointsController : ControllerBase
    {
        private readonly IEndpointService _service;

        public EndpointsController(IEndpointService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] LinkEndpointsToProject request)
        {
            await _service.Create(request.Endpoints, request.ProjectId);
            return Ok();
        }

        [HttpPost("delete-many")]
        public async Task<IActionResult> Delete([FromBody] DeleteManyRequest request)
        {
            await _service.Delete(request.Ids);
            return Ok();
        }

        [Public]
        [HttpPost("search-many")]
        public async Task<IActionResult> GetMany([FromBody] GetManyEndpointRequest request)
        {
            var repsonse = await _service.GetMany(request);
            return Ok(repsonse);
        }
    }
}
