using Microsoft.EntityFrameworkCore;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Domain.Entities;
using VentionTask1.Infrastructure.Data;

namespace VentionTask1.Infrastructure.Repositories.Implementation
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public OrganizationRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Organization>> GetOrganizationsPaginatedAsync(Guid? cursor, int pageSize, CancellationToken ct)
        {
            var query = _dbContext.Organizations
                .AsNoTracking()
                .OrderBy(org => org.Id)
                .AsQueryable();

            if (cursor.HasValue)
            {
                query = query.Where(org => org.Id > cursor.Value);
            }

            return await query
                .Take(pageSize + 1)
                .ToListAsync(ct);
        }

        public async Task<Organization?> GetOrganizationByIdAsync(Guid id, CancellationToken ct)
        {
            return await _dbContext.Organizations
                .FirstOrDefaultAsync(org => org.Id == id, ct);
        }

        public async Task<Organization?> GetOrganizationByNameAsync(string name, CancellationToken ct)
        {
            return await _dbContext.Organizations
                .AsNoTracking()
                .FirstOrDefaultAsync(org => org.Name == name, ct);
        }

        public async Task<Organization> CreateOrganizationAsync(Organization organization, CancellationToken ct)
        {
            await _dbContext.Organizations.AddAsync(organization, ct);

            return organization;
        }

        public Task UpdateOrganizationAsync(Organization organization)
        {
            _dbContext.Organizations.Update(organization);

            return Task.CompletedTask;
        }

        public Task DeleteOrganizationAsync(Organization organization)
        {
            _dbContext.Organizations.Remove(organization);

            return Task.CompletedTask;
        }

        public async Task<bool> SaveChangesAsync(CancellationToken ct)
        {
            return (await _dbContext.SaveChangesAsync(ct)) > 0;
        }
    }
}
