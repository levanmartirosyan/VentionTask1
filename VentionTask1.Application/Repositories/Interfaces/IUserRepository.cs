using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetUsersPaginatedAsync(Guid? cursor, int pageSize, CancellationToken ct);
        Task<User?> GetUserByIdAsync(Guid id, CancellationToken ct);
        Task<User?> GetUserByEmailAsync(string email, CancellationToken ct);
        Task<User> CreateUserAsync(User user, CancellationToken ct);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(User user);
        Task<bool> SaveChangesAsync(CancellationToken ct);
    }
}
