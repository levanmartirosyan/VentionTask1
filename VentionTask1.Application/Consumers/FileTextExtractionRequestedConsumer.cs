using MassTransit;
using Microsoft.Extensions.Logging;
using VentionTask1.Application.Messaging;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;

namespace VentionTask1.Application.Consumers
{
    public class FileTextExtractionRequestedConsumer
        : IConsumer<FileTextExtractionRequestedEvent>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IFileIngestionService _fileIngestionService;
        private readonly IFileProcessingNotifier _notifier;
        private readonly ILogger<FileTextExtractionRequestedConsumer> _logger;

        public FileTextExtractionRequestedConsumer(
            IFileRepository fileRepository,
            IPublishEndpoint publishEndpoint,
            IFileIngestionService fileIngestionService,
            IFileProcessingNotifier notifier,
            ILogger<FileTextExtractionRequestedConsumer> logger)
        {
            _fileRepository = fileRepository;
            _publishEndpoint = publishEndpoint;
            _fileIngestionService = fileIngestionService;
            _notifier = notifier;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<FileTextExtractionRequestedEvent> context)
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
                _logger.LogInformation("Text extraction started for file {FileId}", message.FileId);

                await _notifier.NotifyTextExtractionStartedAsync(message.FileId, file.OrganizationId, ct);

                await _fileIngestionService.IngestAsync(message.FileId, ct);

                await _publishEndpoint.Publish(
                    new FileChunkingRequestedEvent(message.FileId),
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Text extraction failed for file {FileId}", message.FileId);

                await _fileRepository.MarkFailedAsync(message.FileId, ex.Message, ct);

                await _notifier.NotifyFailedAsync(message.FileId, file.OrganizationId, ex.Message, ct);

                throw;
            }
        }
    }
}
