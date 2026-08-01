using Microsoft.EntityFrameworkCore;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Infrastructure.Repositories.Implementation
{
    public class FileChunkRepository : IFileChunkRepository
    {
        private readonly IApplicationDbContext _dbContext;

        public FileChunkRepository(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddRangeAsync(List<FileChunk> chunks, CancellationToken ct)
        {
            await _dbContext.FileChunks.AddRangeAsync(chunks, ct);
        }

        public async Task DeleteByFileIdAsync(Guid fileId, CancellationToken ct)
        {
            var chunks = await _dbContext.FileChunks
                .Where(chunk => chunk.FileId == fileId)
                .ToListAsync(ct);

            _dbContext.FileChunks.RemoveRange(chunks);
        }

        public async Task<bool> SaveChangesAsync(CancellationToken ct)
        {
            return await _dbContext.SaveChangesAsync(ct) > 0;
        }
    }
}