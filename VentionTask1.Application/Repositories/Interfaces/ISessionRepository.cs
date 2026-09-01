using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Repositories.Interfaces
{
    public interface ISessionRepository
    {
        Task AddAsync(Session session, CancellationToken ct);
        Task<bool> SaveChangesAsync(CancellationToken ct);
    }
}
