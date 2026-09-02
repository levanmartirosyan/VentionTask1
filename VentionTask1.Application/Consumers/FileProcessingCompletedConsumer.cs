using MassTransit;
using VentionTask1.Application.Messaging;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;

namespace VentionTask1.Application.Consumers
{
    public class FileProcessingCompletedConsumer
        : IConsumer<FileProcessingCompletedEvent>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IFileProcessingNotifier _notifier;

        public FileProcessingCompletedConsumer(IFileRepository fileRepository, IFileProcessingNotifier notifier)
        {
            _fileRepository = fileRepository;
            _notifier = notifier;
        }

        public async Task Consume(ConsumeContext<FileProcessingCompletedEvent> context)
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
                file.Status = "processed";
                file.ProcessingError = null;

                await _fileRepository.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                file.Status = "failed";
                file.ProcessingError = ex.Message;

                await _fileRepository.SaveChangesAsync(ct);

                await _notifier.NotifyFailedAsync(message.FileId, file.OrganizationId, ex.Message, ct);

                throw;
            }

            await _notifier.NotifyCompletedAsync(message.FileId, file.OrganizationId, ct);
        }
    }
}
