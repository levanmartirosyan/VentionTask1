namespace VentionTask1.Application.Services.Interfaces
{
    public interface IFileIngestionService
    {
        Task IngestAsync(Guid fileId, CancellationToken ct);
    }
}
