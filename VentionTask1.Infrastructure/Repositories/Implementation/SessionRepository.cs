using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Infrastructure.Repositories.Implementation
{
    public class SessionRepository : ISessionRepository
    {
        private readonly IApplicationDbContext _dbContext;

        public SessionRepository(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Session session, CancellationToken ct)
        {
            await _dbContext.Sessions.AddAsync(session, ct);
        }

        public async Task<bool> SaveChangesAsync(CancellationToken ct)
        {
            return await _dbContext.SaveChangesAsync(ct) > 0;
        }
    }
}
