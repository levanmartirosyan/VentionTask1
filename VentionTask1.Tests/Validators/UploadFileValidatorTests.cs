using Microsoft.AspNetCore.Http;
using Moq;
using VentionTask1.Application.DTOs;
using VentionTask1.Application.Validators.File;
using VentionTask1.Domain.Constants;

namespace VentionTask1.Tests.Validators
{
    public class UploadFileValidatorTests
    {
        [Fact]
        public void UploadFileValidator_WhenFileIsValid_ShouldBeValid()
        {
            var validator = new UploadFileDTOValidator();
            var dto = new UploadFileDTO
            {
                File = CreateFile("test.txt", "text/plain", 100)
            };

            var result = validator.Validate(dto);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void UploadFileValidator_WhenFileIsEmpty_ShouldBeInvalid()
        {
            var validator = new UploadFileDTOValidator();
            var dto = new UploadFileDTO
            {
                File = CreateFile("test.txt", "text/plain", 0)
            };

            var result = validator.Validate(dto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.ErrorMessage == "File is required.");
        }

        [Fact]
        public void UploadFileValidator_WhenFileIsTooLarge_ShouldBeInvalid()
        {
            var validator = new UploadFileDTOValidator();
            var dto = new UploadFileDTO
            {
                File = CreateFile("test.txt", "text/plain", FileUploadConstants.MaxFileSize + 1)
            };

            var result = validator.Validate(dto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.ErrorMessage == "File size must be 50 MB or less.");
        }

        [Fact]
        public void UploadFileValidator_WhenExtensionIsNotAllowed_ShouldBeInvalid()
        {
            var validator = new UploadFileDTOValidator();
            var dto = new UploadFileDTO
            {
                File = CreateFile("malware.exe", "application/octet-stream", 100)
            };

            var result = validator.Validate(dto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.ErrorMessage == "File extension is not allowed.");
        }

        [Fact]
        public void UploadFileValidator_WhenMimeTypeDoesNotMatchExtension_ShouldBeInvalid()
        {
            var validator = new UploadFileDTOValidator();
            var dto = new UploadFileDTO
            {
                File = CreateFile("image.png", "text/plain", 100)
            };

            var result = validator.Validate(dto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.ErrorMessage == "File MIME type is not allowed.");
        }

        private static IFormFile CreateFile(string fileName, string contentType, long length)
        {
            var fileMock = new Mock<IFormFile>();

            fileMock
                .Setup(file => file.FileName)
                .Returns(fileName);

            fileMock
                .Setup(file => file.ContentType)
                .Returns(contentType);

            fileMock
                .Setup(file => file.Length)
                .Returns(length);

            return fileMock.Object;
        }
    }
}
