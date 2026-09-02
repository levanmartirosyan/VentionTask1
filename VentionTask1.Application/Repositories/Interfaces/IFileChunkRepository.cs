using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Repositories.Interfaces
{
    public interface IFileChunkRepository
    {
        Task AddRangeAsync(List<FileChunk> chunks, CancellationToken ct);
        Task DeleteByFileIdAsync(Guid fileId, CancellationToken ct);
        Task<bool> SaveChangesAsync(CancellationToken ct);
    }
}
