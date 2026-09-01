using Moq;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Implementation;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Tests.Services
{
    public class FileIngestionServiceTests : IDisposable
    {
        private readonly Mock<IFileRepository> _fileRepositoryMock;
        private readonly Mock<IFileChunkRepository> _fileChunkRepositoryMock;
        private readonly FileIngestionService _service;
        private readonly List<string> _createdFiles = [];

        public FileIngestionServiceTests()
        {
            _fileRepositoryMock = new Mock<IFileRepository>();
            _fileChunkRepositoryMock = new Mock<IFileChunkRepository>();
            _service = new FileIngestionService(_fileRepositoryMock.Object, _fileChunkRepositoryMock.Object);
        }

        [Fact]
        public async Task IngestAsync_WhenFileDoesNotExistInDatabase_ShouldThrowKeyNotFoundException()
        {
            var fileId = Guid.NewGuid();

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(fileId, CancellationToken.None))
                .ReturnsAsync((UploadedFile?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.IngestAsync(fileId, CancellationToken.None));
        }

        [Fact]
        public async Task IngestAsync_WhenPhysicalFileDoesNotExist_ShouldThrowFileNotFoundException()
        {
            var fileId = Guid.NewGuid();
            var file = CreateUploadedFile(fileId, "files/missing.txt");

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(fileId, CancellationToken.None))
                .ReturnsAsync(file);

            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                _service.IngestAsync(fileId, CancellationToken.None));
        }

        [Fact]
        public async Task IngestAsync_WhenTextFileExists_ShouldCreateChunks()
        {
            var fileId = Guid.NewGuid();
            var storageKey = $"files/{Guid.NewGuid()}.txt";
            var fullPath = CreateUploadFile(storageKey, new string('a', 2100));
            var file = CreateUploadedFile(fileId, storageKey);

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(fileId, CancellationToken.None))
                .ReturnsAsync(file);

            List<FileChunk>? savedChunks = null;

            _fileChunkRepositoryMock
                .Setup(repository => repository.AddRangeAsync(It.IsAny<List<FileChunk>>(), CancellationToken.None))
                .Callback<List<FileChunk>, CancellationToken>((chunks, _) => savedChunks = chunks)
                .Returns(Task.CompletedTask);

            _fileChunkRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            await _service.IngestAsync(fileId, CancellationToken.None);

            Assert.NotNull(savedChunks);
            Assert.Equal(3, savedChunks.Count);
            Assert.Equal(0, savedChunks[0].ChunkIndex);
            Assert.Equal(1, savedChunks[1].ChunkIndex);
            Assert.Equal(2, savedChunks[2].ChunkIndex);
            Assert.Equal(fullPath, Path.GetFullPath(fullPath));
            _fileChunkRepositoryMock.Verify(repository => repository.DeleteByFileIdAsync(fileId, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task IngestAsync_WhenFileTypeIsNotSupported_ShouldThrowNotSupportedException()
        {
            var fileId = Guid.NewGuid();
            var storageKey = $"files/{Guid.NewGuid()}.jpg";
            CreateUploadFile(storageKey, "image-content");
            var file = CreateUploadedFile(fileId, storageKey);
            file.Filename = "image.jpg";
            file.ContentType = "image/jpeg";

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(fileId, CancellationToken.None))
                .ReturnsAsync(file);

            await Assert.ThrowsAsync<NotSupportedException>(() =>
                _service.IngestAsync(fileId, CancellationToken.None));
        }

        public void Dispose()
        {
            foreach (var file in _createdFiles)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        private string CreateUploadFile(string storageKey, string content)
        {
            var uploadRoot = Path.GetFullPath(
                Path.Combine(Directory.GetCurrentDirectory(), "uploads"));

            var fullPath = Path.GetFullPath(
                Path.Combine(uploadRoot, storageKey));

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, content);
            _createdFiles.Add(fullPath);

            return fullPath;
        }

        private static UploadedFile CreateUploadedFile(Guid fileId, string storageKey)
        {
            return new UploadedFile
            {
                Id = fileId,
                Filename = Path.GetFileName(storageKey),
                StoredFileName = Path.GetFileName(storageKey),
                Size = 100,
                Status = "processing",
                ContentType = "text/plain",
                Checksum = "checksum",
                StorageKey = storageKey
            };
        }
    }
}
