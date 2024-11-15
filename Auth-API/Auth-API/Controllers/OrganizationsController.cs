using Auth_API.DTOs;
using Auth_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth_API.Controllers
{
    [Route("organizations")]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationsController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateOrganizationRequest request)
        {
            await _organizationService.Create(request);
            return Ok();
        }

        [HttpPost("link-users")]
        public async Task<IActionResult> LinkUsers([FromBody] LinkUserToOraganizationRequest request)
        {
            await _organizationService.LinkUsers(request);
            return Ok();
        }

        [HttpPost("unlink-users")]
        public async Task<IActionResult> UnlinkUsers([FromBody] UnlinkUserToOraganizationRequest request)
        {
            await _organizationService.UnlinkUsers(request);
            return Ok();
        }
    }
}
