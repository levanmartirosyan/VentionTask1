using VentionTask1.Application.DTOs;
using VentionTask1.Application.DTOs.Membership;

namespace VentionTask1.Application.Services.Interfaces
{
    public interface IOrganizationMemberService
    {
        Task<PaginatedResponseDTO<OrganizationMemberDTO>> GetMembersAsync(Guid organizationId, Guid? cursor, int pageSize, CancellationToken ct);
        Task<OrganizationMemberDTO> AddMemberAsync(Guid organizationId, AddOrganizationMemberDTO dto, CancellationToken ct);
        Task<OrganizationMemberDTO> UpdateMemberRoleAsync(Guid organizationId, Guid userId, UpdateOrganizationMemberRoleDTO dto, CancellationToken ct);
        Task RemoveMemberAsync(Guid organizationId, Guid userId, CancellationToken ct);
    }
}
