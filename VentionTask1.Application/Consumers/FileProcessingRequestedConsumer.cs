using MassTransit;
using Microsoft.Extensions.Logging;
using VentionTask1.Application.Messaging;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;

namespace VentionTask1.Application.Consumers
{
    public class FileProcessingRequestedConsumer
        : IConsumer<FileProcessingRequestedEvent>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IFileProcessingNotifier _notifier;
        private readonly ILogger<FileProcessingRequestedConsumer> _logger;

        public FileProcessingRequestedConsumer(
            IFileRepository fileRepository,
            IPublishEndpoint publishEndpoint,
            IFileProcessingNotifier notifier,
            ILogger<FileProcessingRequestedConsumer> logger)
        {
            _fileRepository = fileRepository;
            _publishEndpoint = publishEndpoint;
            _notifier = notifier;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<FileProcessingRequestedEvent> context)
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
                _logger.LogInformation("File processing started for file {FileId}", message.FileId);

                file.Status = "processing";
                file.ProcessingError = null;

                await _fileRepository.SaveChangesAsync(ct);

                await _notifier.NotifyProcessingStartedAsync(file.Id, file.OrganizationId, ct);

                await _publishEndpoint.Publish(
                    new FileTextExtractionRequestedEvent(message.FileId),
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File processing request failed for file {FileId}", message.FileId);

                await _fileRepository.MarkFailedAsync(message.FileId, ex.Message, ct);

                await _notifier.NotifyFailedAsync(message.FileId, file.OrganizationId, ex.Message, ct);

                throw;
            }
        }
    }
}
