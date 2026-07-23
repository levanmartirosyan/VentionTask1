using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Repositories.Interfaces
{
    public interface IFileRepository
    {
        Task<List<UploadedFile>> GetFilesPaginatedAsync(Guid? organizationId, Guid? cursor, int pageSize, CancellationToken ct); 
        Task<UploadedFile?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<UploadedFile> AddAsync(UploadedFile file, CancellationToken ct);
        Task DeleteAsync(UploadedFile file);
        Task<bool> SaveChangesAsync(CancellationToken ct);
    }
}
