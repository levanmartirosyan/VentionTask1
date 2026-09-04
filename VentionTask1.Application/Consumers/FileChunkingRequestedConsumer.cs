using MassTransit;
using Microsoft.Extensions.Logging;
using VentionTask1.Application.Messaging;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;

namespace VentionTask1.Application.Consumers
{
    public class FileChunkingRequestedConsumer
        : IConsumer<FileChunkingRequestedEvent>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IFileProcessingNotifier _notifier;
        private readonly ILogger<FileChunkingRequestedConsumer> _logger;

        public FileChunkingRequestedConsumer(
            IFileRepository fileRepository,
            IPublishEndpoint publishEndpoint,
            IFileProcessingNotifier notifier,
            ILogger<FileChunkingRequestedConsumer> logger)
        {
            _fileRepository = fileRepository;
            _publishEndpoint = publishEndpoint;
            _notifier = notifier;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<FileChunkingRequestedEvent> context)
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
                _logger.LogInformation("Chunking started for file {FileId}", message.FileId);

                await _notifier.NotifyChunkingStartedAsync(message.FileId, file.OrganizationId, ct);

                await _publishEndpoint.Publish(
                    new FileProcessingCompletionRequestedEvent(message.FileId),
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chunking failed for file {FileId}", message.FileId);

                await _fileRepository.MarkFailedAsync(message.FileId, ex.Message, ct);

                await _notifier.NotifyFailedAsync(message.FileId, file.OrganizationId, ex.Message, ct);

                throw;
            }
        }
    }
}
