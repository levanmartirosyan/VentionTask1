using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Moq;
using VentionTask1.Application.DTOs;
using VentionTask1.Application.Exceptions;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Implementation;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Tests.Services
{
    public class FileUploadServiceTests : IDisposable
    {
        private readonly Mock<IFileRepository> _fileRepositoryMock;
        private readonly Mock<IValidator<UploadFileDTO>> _uploadValidatorMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly FileUploadService _service;
        private readonly List<string> _createdFiles = [];

        public FileUploadServiceTests()
        {
            _fileRepositoryMock = new Mock<IFileRepository>();
            _uploadValidatorMock = new Mock<IValidator<UploadFileDTO>>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _uploadValidatorMock
                .Setup(validator => validator.ValidateAsync(It.IsAny<UploadFileDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _service = new FileUploadService(
                _fileRepositoryMock.Object,
                _uploadValidatorMock.Object,
                _publishEndpointMock.Object,
                _httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task GetFilesPaginatedAsync_WhenRepositoryReturnsMoreThanPageSize_ShouldReturnPageAndNextCursor()
        {
            var organizationId = Guid.NewGuid();
            var files = new List<UploadedFile>
            {
                CreateUploadedFile("file-1.txt", organizationId),
                CreateUploadedFile("file-2.txt", organizationId),
                CreateUploadedFile("file-3.txt", organizationId)
            };

            _fileRepositoryMock
                .Setup(repository => repository.GetFilesPaginatedAsync(organizationId, null, 2, CancellationToken.None))
                .ReturnsAsync(files);

            var result = await _service.GetFilesPaginatedAsync(organizationId, null, 2, CancellationToken.None);

            Assert.Equal(2, result.Items.Count);
            Assert.True(result.HasNextPage);
            Assert.Equal(files[1].Id, result.NextCursor);
        }

        [Fact]
        public async Task UploadAsync_WhenValidationFails_ShouldThrowValidationException()
        {
            var dto = new UploadFileDTO { File = CreateFormFile("test.txt", "hello") };
            var validationResult = new FluentValidation.Results.ValidationResult(
            [
                new FluentValidation.Results.ValidationFailure("File", "File is invalid.")
            ]);

            _uploadValidatorMock
                .Setup(validator => validator.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            await Assert.ThrowsAsync<ValidationException>(() =>
                _service.UploadAsync(dto, CancellationToken.None));
        }

        [Fact]
        public async Task UploadAsync_WhenFileAndMetadataAreSaved_ShouldReturnFileDto()
        {
            var organizationId = Guid.NewGuid();
            var dto = new UploadFileDTO
            {
                File = CreateFormFile("test.txt", "hello"),
                OrganizationId = organizationId
            };

            UploadedFile? savedFile = null;

            _fileRepositoryMock
                .Setup(repository => repository.AddAsync(It.IsAny<UploadedFile>(), CancellationToken.None))
                .Callback<UploadedFile, CancellationToken>((file, _) =>
                {
                    savedFile = file;
                    _createdFiles.Add(Path.Combine(Directory.GetCurrentDirectory(), "uploads", file.StorageKey));
                })
                .ReturnsAsync((UploadedFile file, CancellationToken _) => file);

            _fileRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            var result = await _service.UploadAsync(dto, CancellationToken.None);

            Assert.NotNull(savedFile);
            Assert.Equal("test.txt", result.Filename);
            Assert.Equal("pending", result.Status);
            Assert.Equal("text/plain", result.ContentType);
            Assert.Equal(organizationId, result.OrganisationId);
            Assert.False(string.IsNullOrWhiteSpace(result.Checksum));
            Assert.StartsWith($"{organizationId}/files/", result.StorageKey);
        }

        [Fact]
        public async Task UploadAsync_WhenMetadataSaveFails_ShouldThrowFileStorageException()
        {
            var dto = new UploadFileDTO { File = CreateFormFile("test.txt", "hello") };

            _fileRepositoryMock
                .Setup(repository => repository.AddAsync(It.IsAny<UploadedFile>(), CancellationToken.None))
                .Callback<UploadedFile, CancellationToken>((file, _) =>
                    _createdFiles.Add(Path.Combine(Directory.GetCurrentDirectory(), "uploads", file.StorageKey)))
                .ReturnsAsync((UploadedFile file, CancellationToken _) => file);

            _fileRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<FileStorageException>(() =>
                _service.UploadAsync(dto, CancellationToken.None));
        }

        [Fact]
        public async Task DeleteAsync_WhenFileDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            var fileId = Guid.NewGuid();

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(fileId, CancellationToken.None))
                .ReturnsAsync((UploadedFile?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.DeleteAsync(fileId, CancellationToken.None));
        }

        [Fact]
        public async Task DeleteAsync_WhenFileExists_ShouldDeleteFileAndMetadata()
        {
            var file = CreateUploadedFile("delete-me.txt", null);
            var fullPath = CreatePhysicalFile(file.StorageKey, "hello");

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(file.Id, CancellationToken.None))
                .ReturnsAsync(file);

            _fileRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            await _service.DeleteAsync(file.Id, CancellationToken.None);

            Assert.False(File.Exists(fullPath));
            _fileRepositoryMock.Verify(repository => repository.DeleteAsync(file), Times.Once);
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

        private static IFormFile CreateFormFile(string fileName, string content)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var fileMock = new Mock<IFormFile>();

            fileMock
                .Setup(file => file.FileName)
                .Returns(fileName);

            fileMock
                .Setup(file => file.Length)
                .Returns(bytes.Length);

            fileMock
                .Setup(file => file.ContentType)
                .Returns("text/plain");

            fileMock
                .Setup(file => file.OpenReadStream())
                .Returns(() => new MemoryStream(bytes));

            return fileMock.Object;
        }

        private string CreatePhysicalFile(string storageKey, string content)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", storageKey);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, content);
            _createdFiles.Add(fullPath);
            return fullPath;
        }

        private static UploadedFile CreateUploadedFile(string fileName, Guid? organizationId)
        {
            return new UploadedFile
            {
                Id = Guid.NewGuid(),
                Filename = fileName,
                StoredFileName = fileName,
                Size = 100,
                Status = "pending",
                ContentType = "text/plain",
                Checksum = "checksum",
                StorageKey = organizationId.HasValue
                    ? $"{organizationId.Value}/files/{fileName}"
                    : $"files/{fileName}",
                OrganizationId = organizationId
            };
        }
    }
}
