using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VentionTask1.Application.DTOs;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Constants;

namespace VentionTask1.WebApi.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileUploadService _fileService;

        public FilesController(IFileUploadService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponseDTO<FileDTO>>> GetFilesAsync([FromQuery] Guid? cursor, [FromQuery] int pageSize = 10, [FromHeader(Name = "x-org-id")] Guid? organizationId = null, CancellationToken ct = default)
        {
            var files= await _fileService.GetFilesPaginatedAsync(organizationId, cursor, pageSize, ct);

            return Ok(files);
        }

        [HttpPost("upload")]
        [RequestSizeLimit(FileUploadConstants.MaxFileSize)]
        [RequestFormLimits(MultipartBodyLengthLimit = FileUploadConstants.MaxFileSize)]
        public async Task<ActionResult<FileDTO>> Upload(IFormFile file, [FromHeader(Name = "x-org-id")] Guid? organizationId, CancellationToken ct)
        {
            var uploadedFile = await _fileService.UploadAsync(
                new UploadFileDTO
                {
                    File = file,
                    OrganizationId = organizationId
                },
                ct);

            return Created($"/api/files/{uploadedFile.Id}", uploadedFile);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _fileService.DeleteAsync(id, ct);

            return NoContent();
        }

        [HttpPost("{id:guid}/process")]
        public async Task<ActionResult<FileDTO>> Process(Guid id, CancellationToken ct)
        {
            var file = await _fileService.MarkProcessingAsync(id, ct);

            return Ok(file);
        }
    }
}
