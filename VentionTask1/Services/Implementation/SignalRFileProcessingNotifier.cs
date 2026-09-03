using Microsoft.AspNetCore.SignalR;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.WebApi.Hubs;

namespace VentionTask1.WebApi.Services.Implementation
{
    public class FileProcessingSignalRNotifier : IFileProcessingNotifier
    {
        private readonly IHubContext<FileProcessingHub> _hubContext;

        public FileProcessingSignalRNotifier(IHubContext<FileProcessingHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task NotifyProcessingStartedAsync(Guid fileId, Guid? organizationId, CancellationToken ct)
        {
            return SendUpdateAsync(fileId, organizationId, "processing", null, ct);
        }

        public Task NotifyTextExtractionStartedAsync(Guid fileId, Guid? organizationId, CancellationToken ct)
        {
            return SendUpdateAsync(fileId, organizationId, "extracting text", null, ct);
        }

        public Task NotifyChunkingStartedAsync(Guid fileId, Guid? organizationId, CancellationToken ct)
        {
            return SendUpdateAsync(fileId, organizationId, "chunking text", null, ct);
        }

        public Task NotifyCompletedAsync(Guid fileId, Guid? organizationId, CancellationToken ct)
        {
            return SendUpdateAsync(fileId, organizationId, "completed", null, ct);
        }

        public Task NotifyFailedAsync(Guid fileId, Guid? organizationId, string error, CancellationToken ct)
        {
            return SendUpdateAsync(fileId, organizationId, "failed", error, ct);
        }

        private Task SendUpdateAsync(Guid fileId, Guid? organizationId, string status, string? error, CancellationToken ct)
        {
            return _hubContext.Clients
                .Group($"file-{fileId}")
                .SendAsync("FileProcessingUpdate", new
                {
                    fileId,
                    organizationId,
                    status,
                    error
                }, ct);
        }
    }
}
