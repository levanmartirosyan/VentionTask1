using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VentionTask1.Application.DTOs;
using VentionTask1.Application.Services.Implementation;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Entities;
using VentionTask1.WebApi.Extensions;

namespace VentionTask1.WebApi.Controllers
{
    [Route("api/organizations")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
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

        [HttpPut("{id}")]
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
    }
}
