using VentionTask1.Application.DTOs;

namespace VentionTask1.Application.Services.Interfaces
{
    public interface IOrganizationService
    {
        Task<PaginatedResponseDTO<OrganizationDTO>> GetOrganizationsPaginatedAsync(Guid? cursor, int pageSize, CancellationToken ct);
        Task<OrganizationDTO?> GetOrganizationByIdAsync(Guid id, CancellationToken ct);
        Task<OrganizationDTO> CreateOrganizationAsync(CreateOrganizationDTO organizationDTO, CancellationToken ct);
        Task<OrganizationDTO> UpdateOrganizationAsync(Guid id, UpdateOrganizationDTO organizationDTO, CancellationToken ct);
        Task<bool> DeleteOrganizationAsync(Guid id, CancellationToken ct);
    }
}
