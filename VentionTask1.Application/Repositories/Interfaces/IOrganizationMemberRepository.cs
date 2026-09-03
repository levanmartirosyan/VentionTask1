using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Repositories.Interfaces
{
    public interface IOrganizationMemberRepository
    {
        Task<List<OrganizationMember>> GetMembersPaginatedAsync(Guid organizationId, Guid? cursor, int pageSize, CancellationToken ct);
        Task<OrganizationMember?> GetByOrganizationAndUserAsync(Guid organizationId, Guid userId, CancellationToken ct);
        Task AddAsync(OrganizationMember member, CancellationToken ct);
        Task DeleteAsync(OrganizationMember member);
        Task<bool> SaveChangesAsync(CancellationToken ct);
    }
}
