using VentionTask1.Application.DTOs;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Repositories.Interfaces
{
    public interface IOrganizationRepository
    {
        Task<List<Organization>> GetOrganizationsPaginatedAsync(Guid? cursor, int pageSize, CancellationToken ct);
        Task<Organization?> GetOrganizationByIdAsync(Guid id, CancellationToken ct);
        Task<Organization?> GetOrganizationByNameAsync(string name, CancellationToken ct);
        Task<Organization> CreateOrganizationAsync(Organization organization, CancellationToken ct);
        Task UpdateOrganizationAsync(Organization organization);
        Task DeleteOrganizationAsync(Organization organization);
        Task<bool> SaveChangesAsync(CancellationToken ct);
    }
}
