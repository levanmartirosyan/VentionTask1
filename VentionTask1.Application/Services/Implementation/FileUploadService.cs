using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using VentionTask1.Application.DTOs;
using VentionTask1.Application.Exceptions;
using VentionTask1.Application.Extensions;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Services.Implementation
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IFileRepository _fileRepository;
        private readonly IValidator<UploadFileDTO> _uploadFileValidator;

        public FileUploadService(IFileRepository fileRepository, IValidator<UploadFileDTO> uploadFileValidator)
        {
            _fileRepository = fileRepository;
            _uploadFileValidator = uploadFileValidator;
        }

        public async Task<PaginatedResponseDTO<FileDTO>> GetFilesPaginatedAsync(Guid? organizationId, Guid? cursor, int pageSize, CancellationToken ct)
        {
            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            var files = await _fileRepository.GetFilesPaginatedAsync(organizationId, cursor, pageSize, ct);

            var hasNextPage = files.Count > pageSize;

            var items = files
                .Take(pageSize)
                .Select(file => file.ToDto())
                .ToList();

            return new PaginatedResponseDTO<FileDTO>
            {
                Items = items,
                HasNextPage = hasNextPage,
                NextCursor = hasNextPage && items.Any() 
                ? items.Last().Id 
                : null
            };
        }

        public async Task<FileDTO> UploadAsync(UploadFileDTO uploadFileDTO, CancellationToken ct)
        {
            var validationResult = await _uploadFileValidator.ValidateAsync(uploadFileDTO);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var file = uploadFileDTO.File;
            var organizationId = uploadFileDTO.OrganizationId;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var storedFileName = $"{Guid.NewGuid()}{extension}";

            var storageKey = organizationId.HasValue ? $"{organizationId.Value}/files/{storedFileName}" : $"files/{storedFileName}";

            var fullPath = GetSafeUploadPath(storageKey);

            var directoryPath = Path.GetDirectoryName(fullPath);

            if (directoryPath == null)
            {
                throw new InvalidOperationException("Unable to create upload path.");
            }

            string checksum;

            try
            {
                Directory.CreateDirectory(directoryPath);

                checksum = await SaveFileAndCalculateChecksumAsync(file, fullPath, ct);
            }
            catch (IOException ex)
            {
                throw new FileStorageException("File could not be saved.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new FileStorageException("File storage access is denied.", ex);
            }

            var uploadedFile = new UploadedFile
            {
                Id = Guid.NewGuid(),
                Filename = Path.GetFileName(file.FileName),
                StoredFileName = storedFileName,
                Size = file.Length,
                Status = "processed",
                ContentType = file.ContentType,
                Checksum = checksum,
                StorageKey = storageKey,
                OrganizationId = organizationId,
                OwnerId = null,
                Application = null,
                ProcessingError = null
            };

            await _fileRepository.AddAsync(uploadedFile, ct);

            if (!await _fileRepository.SaveChangesAsync(ct))
            {
                TryDeletePhysicalFile(fullPath);

                throw new FileStorageException("File was saved, but metadata could not be saved.");
            }

            return uploadedFile.ToDto();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var file = await _fileRepository.GetByIdAsync(id, ct);

            if (file == null)
            {
                throw new KeyNotFoundException($"File with ID '{id}' was not found.");
            }

            var fullPath = GetSafeUploadPath(file.StorageKey);

            try
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (IOException ex)
            {
                throw new FileStorageException("File could not be deleted from storage.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new FileStorageException("File storage access is denied.", ex);
            }

            await _fileRepository.DeleteAsync(file);

            if (!await _fileRepository.SaveChangesAsync(ct))
            {
                throw new InvalidOperationException("Internal server error occurred while deleting file.");
            }
        }

        public async Task<FileDTO> MarkProcessingAsync(Guid id, CancellationToken ct)
        {
            var file = await _fileRepository.GetByIdAsync(id, ct);

            if (file == null)
            {
                throw new KeyNotFoundException($"File with ID '{id}' was not found.");
            }

            file.Status = "processing";
            file.ProcessingError = null;

            if (!await _fileRepository.SaveChangesAsync(ct))
            {
                throw new InvalidOperationException("Internal server error occurred while updating file.");
            }

            return file.ToDto();
        }

        private static async Task<string> SaveFileAndCalculateChecksumAsync(IFormFile file, string fullPath, CancellationToken ct)
        {
            using var sha256 = SHA256.Create();

            await using var source = file.OpenReadStream();
            await using var destination = File.Create(fullPath);

            var buffer = new byte[81920];
            int bytesRead;

            while ((bytesRead = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
            {
                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
                sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
            }

            sha256.TransformFinalBlock([], 0, 0);

            return Convert.ToHexString(sha256.Hash ?? []);
        }

        private static string GetSafeUploadPath(string storageKey)
        {
            var uploadRoot = Path.GetFullPath(
                Path.Combine(Directory.GetCurrentDirectory(), "uploads"));

            var fullPath = Path.GetFullPath(
                Path.Combine(uploadRoot, storageKey));

            if (!fullPath.StartsWith(uploadRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid file path.");
            }

            return fullPath;
        }

        private static void TryDeletePhysicalFile(string fullPath)
        {
            try
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch
            {
            }
        }
    }
}
