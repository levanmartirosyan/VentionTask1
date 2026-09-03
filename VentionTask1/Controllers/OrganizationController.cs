using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VentionTask1.Application.DTOs;
using VentionTask1.Application.DTOs.Membership;
using VentionTask1.Application.Services.Interfaces;

namespace VentionTask1.WebApi.Controllers
{
    [Authorize]
    [Route("api/organizations")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationMemberService _organizationMemberService;

        public OrganizationController(
            IOrganizationService organizationService,
            IOrganizationMemberService organizationMemberService)
        {
            _organizationService = organizationService;
            _organizationMemberService = organizationMemberService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrganizationsPageAsync([FromQuery] Guid? cursor, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        {
            var result = await _organizationService.GetOrganizationsPaginatedAsync(cursor, pageSize, ct);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrganizationById(Guid id, CancellationToken ct)
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(id, ct);

            return Ok(organization);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrganization(CreateOrganizationDTO organizationDTO, CancellationToken ct)
        {
            var createdOrganization = await _organizationService.CreateOrganizationAsync(organizationDTO, ct);

            return Ok(createdOrganization);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateOrganization(Guid id, UpdateOrganizationDTO organizationDTO, CancellationToken ct)
        {
            var updatedOrganization = await _organizationService.UpdateOrganizationAsync(id, organizationDTO, ct);

            return Ok(updatedOrganization);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganization(Guid id, CancellationToken ct)
        {
            await _organizationService.DeleteOrganizationAsync(id, ct);

            return NoContent();
        }

        [HttpGet("{organizationId}/members")]
        public async Task<IActionResult> GetMembersAsync(Guid organizationId, [FromQuery] Guid? cursor, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        {
            var result = await _organizationMemberService.GetMembersAsync(organizationId, cursor, pageSize, ct);

            return Ok(result);
        }

        [HttpPost("{organizationId}/members")]
        public async Task<IActionResult> AddMemberAsync(Guid organizationId, [FromBody] AddOrganizationMemberDTO dto, CancellationToken ct)
        {
            var result = await _organizationMemberService.AddMemberAsync(organizationId, dto, ct);

            return Ok(result);
        }

        [HttpPatch("{organizationId}/members/{userId}")]
        public async Task<IActionResult> UpdateMemberRoleAsync(Guid organizationId, Guid userId, [FromBody] UpdateOrganizationMemberRoleDTO dto, CancellationToken ct)
        {
            var result = await _organizationMemberService.UpdateMemberRoleAsync(organizationId, userId, dto, ct);

            return Ok(result);
        }

        [HttpDelete("{organizationId}/members/{userId}")]
        public async Task<IActionResult> RemoveMemberAsync(Guid organizationId, Guid userId, CancellationToken ct)
        {
            await _organizationMemberService.RemoveMemberAsync(organizationId, userId, ct);

            return NoContent();
        }
    }
}
