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

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _dbContext.Users
                .Include(user => user.Organization)
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _dbContext.Users
                .Include(user => user.Organization)
                .FirstOrDefaultAsync(user => user.Id == id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _dbContext.Users
                .Include(user => user.Organization)
                .FirstOrDefaultAsync(user => user.Email == email);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);

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

        public async Task<bool> SaveChangesAsync()
        {
            return (await _dbContext.SaveChangesAsync()) > 0;
        }
    }
}
