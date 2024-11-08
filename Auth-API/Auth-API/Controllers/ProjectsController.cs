using Auth_API.DTOs;
using Auth_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth_API.Controllers
{
    [Route("projects")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _service;

        public ProjectsController(IProjectService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
        {
            await _service.Create(request);
            return Ok();
        }

        [HttpPost("upsert")]
        public async Task<IActionResult> Upsert([FromBody] UpsertProjectRequest request)
        {
            await _service.Upsert(request);
            return Ok();
        }

        [HttpPost("search-many")]
        public async Task<IActionResult> GetMany()
        {
            var response = await _service.GetMany();
            return Ok(response);
        }

        [HttpPost("search-one")]
        public async Task<IActionResult> Get([FromBody] GetOneProjectRequest request)
        {
            var response = await _service.Get(request.Id);
            return Ok(response);
        }

        [HttpPost("delete-one")]
        public async Task<IActionResult> Delete([FromBody] DeleteOneProjectRequest request)
        {
            await _service.Delete(request.Id);
            return Ok();
        }
    }
}
