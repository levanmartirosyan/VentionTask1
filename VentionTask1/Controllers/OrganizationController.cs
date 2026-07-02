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
    [Route("api/Organization")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrganizations(CancellationToken ct)
        {
            var organizations = await _organizationService.GetAllOrganizationsAsync(ct);

            if (organizations == null || !organizations.Any())
            {
                return NoContent();
            }

            return Ok(organizations);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrganizationById(Guid id, CancellationToken ct)
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(id, ct);

            if (organization == null)
            {
                return NotFound();
            }

            return Ok(organization);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrganization(CreateOrganizationDTO organizationDTO, CancellationToken ct)
        {
            try
            {
                var createdOrganization = await _organizationService.CreateOrganizationAsync(organizationDTO, ct);

                return Ok(createdOrganization);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.ToErrorDictionary());
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    Message = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrganization(Guid id, UpdateOrganizationDTO organizationDTO, CancellationToken ct)
        {
            try
            {
                var updatedOrganization = await _organizationService.UpdateOrganizationAsync(id, organizationDTO, ct);

                return Ok(updatedOrganization);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.ToErrorDictionary());
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    Message = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganization(Guid id, CancellationToken ct)
        {
            try
            {
                var result = await _organizationService.DeleteOrganizationAsync(id, ct);

                if (!result)
                {
                    return NotFound();
                }

                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
