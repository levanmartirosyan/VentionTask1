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
    public class FileProcessingRequestedConsumerTests
    {
        private readonly Mock<IFileRepository> _fileRepositoryMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly Mock<IFileProcessingNotifier> _notifierMock;
        private readonly Mock<ILogger<FileProcessingRequestedConsumer>> _loggerMock;
        private readonly FileProcessingRequestedConsumer _consumer;

        public FileProcessingRequestedConsumerTests()
        {
            _fileRepositoryMock = new Mock<IFileRepository>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _notifierMock = new Mock<IFileProcessingNotifier>();
            _loggerMock = new Mock<ILogger<FileProcessingRequestedConsumer>>();

            _consumer = new FileProcessingRequestedConsumer(
                _fileRepositoryMock.Object,
                _publishEndpointMock.Object,
                _notifierMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Consume_WhenFileDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            var message = CreateMessage(Guid.NewGuid());
            var context = CreateContext(message);

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(message.FileId, CancellationToken.None))
                .ReturnsAsync((UploadedFile?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _consumer.Consume(context));
        }

        [Fact]
        public async Task Consume_WhenFileAlreadyProcessed_ShouldNotPublishNextEvent()
        {
            var file = CreateFile("processed");
            var message = CreateMessage(file.Id);
            var context = CreateContext(message);

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(file.Id, CancellationToken.None))
                .ReturnsAsync(file);

            await _consumer.Consume(context);

            _publishEndpointMock.Verify(
                endpoint => endpoint.Publish(It.IsAny<FileTextExtractionRequestedEvent>(), CancellationToken.None),
                Times.Never);
        }

        [Fact]
        public async Task Consume_WhenFileExists_ShouldMarkProcessingNotifyAndPublishTextExtractedEvent()
        {
            var file = CreateFile("pending");
            var message = CreateMessage(file.Id);
            var context = CreateContext(message);

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(file.Id, CancellationToken.None))
                .ReturnsAsync(file);

            _fileRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            await _consumer.Consume(context);

            Assert.Equal("processing", file.Status);
            Assert.Null(file.ProcessingError);

            _notifierMock.Verify(
                notifier => notifier.NotifyProcessingStartedAsync(file.Id, file.OrganizationId, CancellationToken.None),
                Times.Once);

            _publishEndpointMock.Verify(
                endpoint => endpoint.Publish(
                    It.Is<FileTextExtractionRequestedEvent>(eventMessage => eventMessage.FileId == file.Id),
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task Consume_WhenPublishFails_ShouldMarkFailedNotifyAndRethrow()
        {
            var file = CreateFile("pending");
            var message = CreateMessage(file.Id);
            var context = CreateContext(message);
            var exception = new InvalidOperationException("Publish failed.");

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(file.Id, CancellationToken.None))
                .ReturnsAsync(file);

            _fileRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            _publishEndpointMock
                .Setup(endpoint => endpoint.Publish(It.IsAny<FileTextExtractionRequestedEvent>(), CancellationToken.None))
                .ThrowsAsync(exception);

            var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _consumer.Consume(context));

            Assert.Equal(exception.Message, thrown.Message);

            _fileRepositoryMock.Verify(
                repository => repository.MarkFailedAsync(file.Id, exception.Message, CancellationToken.None),
                Times.Once);

            _notifierMock.Verify(
                notifier => notifier.NotifyFailedAsync(file.Id, file.OrganizationId, exception.Message, CancellationToken.None),
                Times.Once);
        }

        private static ConsumeContext<FileProcessingRequestedEvent> CreateContext(FileProcessingRequestedEvent message)
        {
            var contextMock = new Mock<ConsumeContext<FileProcessingRequestedEvent>>();

            contextMock
                .SetupGet(context => context.Message)
                .Returns(message);

            contextMock
                .SetupGet(context => context.CancellationToken)
                .Returns(CancellationToken.None);

            return contextMock.Object;
        }

        private static FileProcessingRequestedEvent CreateMessage(Guid fileId)
        {
            return new FileProcessingRequestedEvent(
                fileId,
                Guid.NewGuid(),
                "files/test.txt",
                "text/plain");
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
