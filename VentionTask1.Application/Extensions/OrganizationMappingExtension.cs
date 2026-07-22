using VentionTask1.Application.DTOs;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Extensions
{
    public static class OrganizationMappingExtensions
    {
        public static OrganizationDTO ToDto(this Organization organization)
        {
            return new OrganizationDTO
            {
                Id = organization.Id,
                Name = organization.Name,
                CreatedAt = organization.CreatedAt,
                UpdatedAt = organization.UpdatedAt
            };
        }

        public static Organization ToEntity(this CreateOrganizationDTO dto)
        {
            return new Organization
            {
                Name = dto.Name.Trim()
            };
        }
    }
}
