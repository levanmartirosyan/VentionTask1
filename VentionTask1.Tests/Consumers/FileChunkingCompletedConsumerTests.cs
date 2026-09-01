using MassTransit;
using Moq;
using VentionTask1.Application.Consumers;
using VentionTask1.Application.Messaging;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Tests.Consumers
{
    public class FileChunkingCompletedConsumerTests
    {
        private readonly Mock<IFileRepository> _fileRepositoryMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly Mock<IFileProcessingNotifier> _notifierMock;
        private readonly FileChunkingCompletedConsumer _consumer;

        public FileChunkingCompletedConsumerTests()
        {
            _fileRepositoryMock = new Mock<IFileRepository>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _notifierMock = new Mock<IFileProcessingNotifier>();

            _consumer = new FileChunkingCompletedConsumer(
                _fileRepositoryMock.Object,
                _publishEndpointMock.Object,
                _notifierMock.Object);
        }

        [Fact]
        public async Task Consume_WhenFileExists_ShouldNotifyChunkingAndPublishProcessingCompletedEvent()
        {
            var file = CreateFile("processing");
            var message = new FileChunkingCompletedEvent(file.Id);
            var context = CreateContext(message);

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(file.Id, CancellationToken.None))
                .ReturnsAsync(file);

            await _consumer.Consume(context);

            _notifierMock.Verify(
                notifier => notifier.NotifyChunkingStartedAsync(file.Id, file.OrganizationId, CancellationToken.None),
                Times.Once);

            _publishEndpointMock.Verify(
                endpoint => endpoint.Publish(
                    It.Is<FileProcessingCompletedEvent>(eventMessage => eventMessage.FileId == file.Id),
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task Consume_WhenPublishFails_ShouldMarkFailedNotifyAndRethrow()
        {
            var file = CreateFile("processing");
            var message = new FileChunkingCompletedEvent(file.Id);
            var context = CreateContext(message);
            var exception = new InvalidOperationException("Publish failed.");

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(file.Id, CancellationToken.None))
                .ReturnsAsync(file);

            _fileRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            _publishEndpointMock
                .Setup(endpoint => endpoint.Publish(It.IsAny<FileProcessingCompletedEvent>(), CancellationToken.None))
                .ThrowsAsync(exception);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _consumer.Consume(context));

            Assert.Equal("failed", file.Status);
            Assert.Equal(exception.Message, file.ProcessingError);

            _notifierMock.Verify(
                notifier => notifier.NotifyFailedAsync(file.Id, file.OrganizationId, exception.Message, CancellationToken.None),
                Times.Once);
        }

        private static ConsumeContext<FileChunkingCompletedEvent> CreateContext(FileChunkingCompletedEvent message)
        {
            var contextMock = new Mock<ConsumeContext<FileChunkingCompletedEvent>>();

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
