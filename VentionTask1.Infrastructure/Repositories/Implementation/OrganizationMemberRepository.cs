using Microsoft.EntityFrameworkCore;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Infrastructure.Repositories.Implementation
{
    public class OrganizationMemberRepository : IOrganizationMemberRepository
    {
        private readonly IApplicationDbContext _dbContext;

        public OrganizationMemberRepository(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<OrganizationMember>> GetMembersPaginatedAsync(
            Guid organizationId,
            Guid? cursor,
            int pageSize,
            CancellationToken ct)
        {
            var query = _dbContext.OrganizationMembers
                .AsNoTracking()
                .Include(member => member.User)
                .Include(member => member.Organization)
                .Where(member => member.OrganizationId == organizationId)
                .OrderBy(member => member.Id)
                .AsQueryable();

            if (cursor.HasValue)
            {
                query = query.Where(member => member.Id > cursor.Value);
            }

            return await query
                .Take(pageSize + 1)
                .ToListAsync(ct);
        }

        public async Task<List<OrganizationMember>> GetUserMembershipsAsync(
            Guid userId,
            CancellationToken ct)
        {
            return await _dbContext.OrganizationMembers
                .AsNoTracking()
                .Include(member => member.Organization)
                .Where(member => member.UserId == userId)
                .OrderBy(member => member.Organization.Name)
                .ToListAsync(ct);
        }

        public async Task<OrganizationMember?> GetByOrganizationAndUserAsync(
            Guid organizationId,
            Guid userId,
            CancellationToken ct)
        {
            return await _dbContext.OrganizationMembers
                .Include(member => member.User)
                .Include(member => member.Organization)
                .FirstOrDefaultAsync(
                    member =>
                        member.OrganizationId == organizationId &&
                        member.UserId == userId,
                    ct);
        }

        public async Task AddAsync(OrganizationMember member, CancellationToken ct)
        {
            await _dbContext.OrganizationMembers.AddAsync(member, ct);
        }

        public Task DeleteAsync(OrganizationMember member)
        {
            _dbContext.OrganizationMembers.Remove(member);

            return Task.CompletedTask;
        }

        public async Task<bool> SaveChangesAsync(CancellationToken ct)
        {
            return await _dbContext.SaveChangesAsync(ct) > 0;
        }
    }
}
