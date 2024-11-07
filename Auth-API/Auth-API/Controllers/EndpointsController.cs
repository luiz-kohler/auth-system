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

        [HttpPost("projects/{projectId}")]
        public async Task<IActionResult> Create([FromBody] List<CreateEndpointRequest> request, int projectId)
        {
            await _service.Create(request, projectId);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            await _service.Delete(ids);
            return Ok();
        }

        [Public]
        [HttpGet]
        public async Task<IActionResult> GetMany([FromQuery] GetManyEndpointRequest request)
        {
            var repsonse = await _service.GetMany(request);
            return Ok(repsonse);
        }
    }
}
