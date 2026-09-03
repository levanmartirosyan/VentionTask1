using Microsoft.AspNetCore.Http;

namespace VentionTask1.Application.DTOs
{
    public class UploadFileDTO
    {
        public required IFormFile File { get; set; }
        public Guid? OrganizationId { get; set; }
    }
}
