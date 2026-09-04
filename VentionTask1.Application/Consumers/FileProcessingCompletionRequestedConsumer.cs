using MassTransit;
using Microsoft.Extensions.Logging;
using VentionTask1.Application.Messaging;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;

namespace VentionTask1.Application.Consumers
{
    public class FileProcessingCompletionRequestedConsumer
        : IConsumer<FileProcessingCompletionRequestedEvent>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IFileProcessingNotifier _notifier;
        private readonly ILogger<FileProcessingCompletionRequestedConsumer> _logger;

        public FileProcessingCompletionRequestedConsumer(IFileRepository fileRepository, IFileProcessingNotifier notifier, ILogger<FileProcessingCompletionRequestedConsumer> logger)
        {
            _fileRepository = fileRepository;
            _notifier = notifier;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<FileProcessingCompletionRequestedEvent> context)
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
                _logger.LogInformation("File processing completion started for file {FileId}", message.FileId);

                file.Status = "processed";
                file.ProcessingError = null;

                await _fileRepository.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File processing completion failed for file {FileId}", message.FileId);

                await _fileRepository.MarkFailedAsync(message.FileId, ex.Message, ct);

                await _notifier.NotifyFailedAsync(message.FileId, file.OrganizationId, ex.Message, ct);

                throw;
            }

            _logger.LogInformation("File processing completed for file {FileId}", message.FileId);

            await _notifier.NotifyCompletedAsync(message.FileId, file.OrganizationId, ct);
        }
    }
}
