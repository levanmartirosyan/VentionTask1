using VentionTask1.Application.DTOs;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Extensions
{
    public static class FileMappingExtensions
    {
        public static FileDTO ToDto(this UploadedFile file)
        {
            return new FileDTO
            {
                Id = file.Id,
                Filename = file.Filename,
                Size = file.Size,
                Status = file.Status,
                ContentType = file.ContentType,
                Checksum = file.Checksum,
                StorageKey = file.StorageKey,
                OrganisationId = file.OrganizationId,
                OwnerId = file.OwnerId,
                Application = file.Application,
                ProcessingError = file.ProcessingError,
                CreatedAt = file.CreatedAt,
                UpdatedAt = file.UpdatedAt
            };
        }
    }
}
