using MassTransit;
using VentionTask1.Application.Messaging;
using VentionTask1.Application.Repositories.Interfaces;

namespace VentionTask1.Application.Consumers
{
    public class FileTextExtractedConsumer
        : IConsumer<FileTextExtractedEvent>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public FileTextExtractedConsumer(
            IFileRepository fileRepository,
            IPublishEndpoint publishEndpoint)
        {
            _fileRepository = fileRepository;
            _publishEndpoint = publishEndpoint;
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
                Console.WriteLine($"Chunking text for file {message.FileId}");

                

                await _publishEndpoint.Publish(
                    new FileChunkingCompletedEvent(message.FileId),
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
