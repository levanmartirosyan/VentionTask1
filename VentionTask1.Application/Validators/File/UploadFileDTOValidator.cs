using FluentValidation;
using Microsoft.AspNetCore.Http;
using VentionTask1.Application.DTOs;
using VentionTask1.Domain.Constants;

namespace VentionTask1.Application.Validators.File
{
    public class UploadFileDTOValidator : AbstractValidator<UploadFileDTO>
    {
        public UploadFileDTOValidator()
        {
            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("File is required.");

            RuleFor(x => x.File.Length)
                .GreaterThan(0)
                .WithMessage("File is required.")
                .LessThanOrEqualTo(FileUploadConstants.MaxFileSize)
                .WithMessage("File size must be 50 MB or less.")
                .When(x => x.File != null);

            RuleFor(x => x.File.FileName)
                .Must(HaveAllowedExtension)
                .WithMessage("File extension is not allowed.")
                .When(x => x.File != null);

            RuleFor(x => x.File)
                .Must(HaveAllowedMimeType)
                .WithMessage("File MIME type is not allowed.")
                .When(x => x.File != null);
        }

        private static bool HaveAllowedExtension(string fileName)
        {
            var extension = Path.GetExtension(fileName);

            return !string.IsNullOrWhiteSpace(extension)
                && FileUploadConstants.AllowedFileTypes.ContainsKey(extension);
        }

        private static bool HaveAllowedMimeType(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);

            return !string.IsNullOrWhiteSpace(extension)
                && FileUploadConstants.AllowedFileTypes.TryGetValue(extension, out var allowedMimeTypes)
                && allowedMimeTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase);
        }
    }
}
