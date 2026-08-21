namespace VentionTask1.Application.Services.Interfaces
{
    public interface IFileProcessingNotifier
    {
        Task NotifyProcessingStartedAsync(Guid fileId, Guid? organizationId, CancellationToken ct);
        Task NotifyTextExtractionStartedAsync(Guid fileId, Guid? organizationId, CancellationToken ct);
        Task NotifyChunkingStartedAsync(Guid fileId, Guid? organizationId, CancellationToken ct);
        Task NotifyCompletedAsync(Guid fileId, Guid? organizationId, CancellationToken ct);
        Task NotifyFailedAsync(Guid fileId, Guid? organizationId, string error, CancellationToken ct);
    }
}
