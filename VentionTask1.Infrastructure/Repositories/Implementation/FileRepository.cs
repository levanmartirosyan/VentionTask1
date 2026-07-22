using Microsoft.EntityFrameworkCore;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Infrastructure.Repositories.Implementation
{
    public class FileRepository : IFileRepository
    {
        private readonly IApplicationDbContext _dbContext;

        public FileRepository(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<UploadedFile>> GetFilesPaginatedAsync(Guid? organizationId, Guid? cursor, int pageSize, CancellationToken ct)
        {
            var query = _dbContext.Files
                .AsNoTracking()
                .OrderByDescending(file => file.CreatedAt)
                .AsQueryable();

            if (organizationId.HasValue)
            {
                query = query.Where(file => file.OrganizationId == organizationId.Value);
            }

            if (cursor.HasValue)
            {
                query = query.Where(file => file.Id < cursor.Value);
            }

            return await query
                .Take(pageSize + 1)
                .ToListAsync(ct);
        }

        public async Task<UploadedFile?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return await _dbContext.Files.FirstOrDefaultAsync(file => file.Id == id);
        }

        public async Task<UploadedFile> AddAsync(UploadedFile file, CancellationToken ct)
        {
            await _dbContext.Files.AddAsync(file, ct);

            return file;
        }

        public Task DeleteAsync(UploadedFile file)
        {
            _dbContext.Files.Remove(file);

            return Task.CompletedTask;
        }

        public async Task<bool> SaveChangesAsync(CancellationToken ct)
        {
            return await _dbContext.SaveChangesAsync(ct) > 0;
        }
    }
}
