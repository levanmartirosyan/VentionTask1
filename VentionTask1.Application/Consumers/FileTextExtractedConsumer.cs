using MassTransit;
using VentionTask1.Application.Messaging;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;

namespace VentionTask1.Application.Consumers
{
    public class FileTextExtractedConsumer
        : IConsumer<FileTextExtractedEvent>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IFileIngestionService _fileIngestionService;
        private readonly IFileProcessingNotifier _notifier;

        public FileTextExtractedConsumer(
            IFileRepository fileRepository,
            IPublishEndpoint publishEndpoint,
            IFileIngestionService fileIngestionService,
            IFileProcessingNotifier notifier)
        {
            _fileRepository = fileRepository;
            _publishEndpoint = publishEndpoint;
            _fileIngestionService = fileIngestionService;
            _notifier = notifier;
        }

        public async Task Consume(ConsumeContext<FileTextExtractedEvent> context)
        {
            var message = context.Message;
            var ct = context.CancellationToken;

            var file = await _fileRepository.GetByIdAsync(message.FileId, ct);

            if (file == null)
            {
                throw new KeyNotFoundException($"File with ID '{message.FileId}' was not found.");
            }

            if (file.Status == "processed")
            {
                return;
            }

            try
            {
                await _notifier.NotifyTextExtractionStartedAsync(message.FileId, file.OrganizationId, ct);

                await _fileIngestionService.IngestAsync(message.FileId, ct);

                await _publishEndpoint.Publish(
                    new FileChunkingCompletedEvent(message.FileId),
                    ct);
            }
            catch (Exception ex)
            {
                file.Status = "failed";
                file.ProcessingError = ex.Message;

                await _fileRepository.SaveChangesAsync(ct);

                await _notifier.NotifyFailedAsync(message.FileId, file.OrganizationId, ex.Message, ct);

                throw;
            }
        }
    }
}
