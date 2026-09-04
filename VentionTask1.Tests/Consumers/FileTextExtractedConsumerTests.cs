using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using VentionTask1.Application.Consumers;
using VentionTask1.Application.Messaging;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Tests.Consumers
{
    public class FileTextExtractedConsumerTests
    {
        private readonly Mock<IFileRepository> _fileRepositoryMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly Mock<IFileIngestionService> _fileIngestionServiceMock;
        private readonly Mock<IFileProcessingNotifier> _notifierMock;
        private readonly Mock<ILogger<FileTextExtractionRequestedConsumer>> _loggerMock;
        private readonly FileTextExtractionRequestedConsumer _consumer;

        public FileTextExtractedConsumerTests()
        {
            _fileRepositoryMock = new Mock<IFileRepository>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _fileIngestionServiceMock = new Mock<IFileIngestionService>();
            _notifierMock = new Mock<IFileProcessingNotifier>();
            _loggerMock = new Mock<ILogger<FileTextExtractionRequestedConsumer>>();

            _consumer = new FileTextExtractionRequestedConsumer(
                _fileRepositoryMock.Object,
                _publishEndpointMock.Object,
                _fileIngestionServiceMock.Object,
                _notifierMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Consume_WhenFileExists_ShouldNotifyExtractingIngestAndPublishChunkingCompletedEvent()
        {
            var file = CreateFile("processing");
            var message = new FileTextExtractionRequestedEvent(file.Id);
            var context = CreateContext(message);

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(file.Id, CancellationToken.None))
                .ReturnsAsync(file);

            await _consumer.Consume(context);

            _notifierMock.Verify(
                notifier => notifier.NotifyTextExtractionStartedAsync(file.Id, file.OrganizationId, CancellationToken.None),
                Times.Once);

            _fileIngestionServiceMock.Verify(
                service => service.IngestAsync(file.Id, CancellationToken.None),
                Times.Once);

            _publishEndpointMock.Verify(
                endpoint => endpoint.Publish(
                    It.Is<FileChunkingRequestedEvent>(eventMessage => eventMessage.FileId == file.Id),
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task Consume_WhenIngestionFails_ShouldMarkFailedNotifyAndRethrow()
        {
            var file = CreateFile("processing");
            var message = new FileTextExtractionRequestedEvent(file.Id);
            var context = CreateContext(message);
            var exception = new NotSupportedException("Unsupported file.");

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(file.Id, CancellationToken.None))
                .ReturnsAsync(file);

            _fileRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            _fileIngestionServiceMock
                .Setup(service => service.IngestAsync(file.Id, CancellationToken.None))
                .ThrowsAsync(exception);

            await Assert.ThrowsAsync<NotSupportedException>(() =>
                _consumer.Consume(context));

            _fileRepositoryMock.Verify(
                repository => repository.MarkFailedAsync(file.Id, exception.Message, CancellationToken.None),
                Times.Once);

            _notifierMock.Verify(
                notifier => notifier.NotifyFailedAsync(file.Id, file.OrganizationId, exception.Message, CancellationToken.None),
                Times.Once);
        }

        private static ConsumeContext<FileTextExtractionRequestedEvent> CreateContext(FileTextExtractionRequestedEvent message)
        {
            var contextMock = new Mock<ConsumeContext<FileTextExtractionRequestedEvent>>();

            contextMock
                .SetupGet(context => context.Message)
                .Returns(message);

            contextMock
                .SetupGet(context => context.CancellationToken)
                .Returns(CancellationToken.None);

            return contextMock.Object;
        }

        private static UploadedFile CreateFile(string status)
        {
            return new UploadedFile
            {
                Id = Guid.NewGuid(),
                Filename = "test.txt",
                StoredFileName = "stored-test.txt",
                Size = 100,
                Status = status,
                ContentType = "text/plain",
                Checksum = "checksum",
                StorageKey = "files/stored-test.txt",
                OrganizationId = Guid.NewGuid()
            };
        }
    }
}
