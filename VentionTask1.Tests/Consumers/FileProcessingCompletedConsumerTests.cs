using MassTransit;
using Moq;
using VentionTask1.Application.Consumers;
using VentionTask1.Application.Messaging;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Tests.Consumers
{
    public class FileProcessingCompletedConsumerTests
    {
        private readonly Mock<IFileRepository> _fileRepositoryMock;
        private readonly Mock<IFileProcessingNotifier> _notifierMock;
        private readonly FileProcessingCompletedConsumer _consumer;

        public FileProcessingCompletedConsumerTests()
        {
            _fileRepositoryMock = new Mock<IFileRepository>();
            _notifierMock = new Mock<IFileProcessingNotifier>();

            _consumer = new FileProcessingCompletedConsumer(
                _fileRepositoryMock.Object,
                _notifierMock.Object);
        }

        [Fact]
        public async Task Consume_WhenFileExists_ShouldMarkProcessedAndNotifyCompleted()
        {
            var file = CreateFile("processing");
            var message = new FileProcessingCompletedEvent(file.Id);
            var context = CreateContext(message);

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(file.Id, CancellationToken.None))
                .ReturnsAsync(file);

            _fileRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            await _consumer.Consume(context);

            Assert.Equal("processed", file.Status);
            Assert.Null(file.ProcessingError);

            _notifierMock.Verify(
                notifier => notifier.NotifyCompletedAsync(file.Id, file.OrganizationId, CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task Consume_WhenFileAlreadyProcessed_ShouldNotSaveOrNotify()
        {
            var file = CreateFile("processed");
            var message = new FileProcessingCompletedEvent(file.Id);
            var context = CreateContext(message);

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(file.Id, CancellationToken.None))
                .ReturnsAsync(file);

            await _consumer.Consume(context);

            _fileRepositoryMock.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Never);
            _notifierMock.Verify(
                notifier => notifier.NotifyCompletedAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), CancellationToken.None),
                Times.Never);
        }

        [Fact]
        public async Task Consume_WhenSaveFails_ShouldMarkFailedNotifyAndRethrow()
        {
            var file = CreateFile("processing");
            var message = new FileProcessingCompletedEvent(file.Id);
            var context = CreateContext(message);
            var exception = new InvalidOperationException("Save failed.");

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(file.Id, CancellationToken.None))
                .ReturnsAsync(file);

            _fileRepositoryMock
                .SetupSequence(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ThrowsAsync(exception)
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _consumer.Consume(context));

            Assert.Equal("failed", file.Status);
            Assert.Equal(exception.Message, file.ProcessingError);

            _notifierMock.Verify(
                notifier => notifier.NotifyFailedAsync(file.Id, file.OrganizationId, exception.Message, CancellationToken.None),
                Times.Once);
        }

        private static ConsumeContext<FileProcessingCompletedEvent> CreateContext(FileProcessingCompletedEvent message)
        {
            var contextMock = new Mock<ConsumeContext<FileProcessingCompletedEvent>>();

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
