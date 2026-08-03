using MassTransit;
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

        public FileProcessingRequestedConsumer(
            IFileRepository fileRepository,
            IPublishEndpoint publishEndpoint)
        {
            _fileRepository = fileRepository;
            _publishEndpoint = publishEndpoint;
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
                file.Status = "processing";
                file.ProcessingError = null;

                await _fileRepository.SaveChangesAsync(ct);

                await _publishEndpoint.Publish(
                    new FileTextExtractedEvent(message.FileId),
                    ct);
            }
            catch (Exception ex)
            {
                file.Status = "failed";
                file.ProcessingError = ex.Message;

                await _fileRepository.SaveChangesAsync(ct);

                throw;
            }
        }
    }
}
