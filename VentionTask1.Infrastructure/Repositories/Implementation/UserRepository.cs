using Microsoft.EntityFrameworkCore;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Domain.Entities;
using VentionTask1.Infrastructure.Data;

namespace VentionTask1.Infrastructure.Repositories.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UserRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<User>> GetUsersPaginatedAsync(Guid? cursor, int pageSize, CancellationToken ct)
        {
            var query = _dbContext.Users
                .AsNoTracking()
                .Include(user => user.Organization)
                .OrderBy(user => user.Id)
                .AsQueryable();

            if (cursor.HasValue)
            {
                query = query.Where(user => user.Id > cursor.Value);
            }

            return await query
                .Take(pageSize + 1)
                .ToListAsync(ct);
        }

        public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken ct)
        {
            return await _dbContext.Users
                .Include(user => user.Organization)
                .FirstOrDefaultAsync(user => user.Id == id, ct);
        }

        public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Include(user => user.Organization)
                .FirstOrDefaultAsync(user => user.Email == email, ct);
        }

        public async Task<User> CreateUserAsync(User user, CancellationToken ct)
        {
            await _dbContext.Users.AddAsync(user, ct);

            return user;
        }

        public Task UpdateUserAsync(User user)
        {
            _dbContext.Users.Update(user);

            return Task.CompletedTask;
        }

        public Task DeleteUserAsync(User user)
        {
            _dbContext.Users.Remove(user);

            return Task.CompletedTask;
        }

        public async Task<bool> SaveChangesAsync(CancellationToken ct)
        {
            return (await _dbContext.SaveChangesAsync(ct)) > 0;
        }
    }
}
