using Microsoft.AspNetCore.Http;
using VentionTask1.Application.DTOs;

namespace VentionTask1.Application.Services.Interfaces
{
    public interface IFileUploadService
    {
        Task<PaginatedResponseDTO<FileDTO>> GetFilesPaginatedAsync(Guid? organizationId, Guid? cursor, int pageSize,CancellationToken ct);
        Task<FileDTO> UploadAsync(UploadFileDTO uploadFileDTO, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
        Task<FileDTO> MarkProcessingAsync(Guid id, CancellationToken ct);
    }
}
