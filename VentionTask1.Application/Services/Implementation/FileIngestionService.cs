using System.Text;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Services.Implementation
{
    public class FileIngestionService : IFileIngestionService
    {
        private const int ChunkSize = 1000;

        private readonly IFileRepository _fileRepository;
        private readonly IFileChunkRepository _fileChunkRepository;

        public FileIngestionService(
            IFileRepository fileRepository,
            IFileChunkRepository fileChunkRepository)
        {
            _fileRepository = fileRepository;
            _fileChunkRepository = fileChunkRepository;
        }

        public async Task IngestAsync(Guid fileId, CancellationToken ct)
        {
            var file = await _fileRepository.GetByIdAsync(fileId, ct);

            if (file == null)
            {
                throw new KeyNotFoundException($"File with ID '{fileId}' was not found.");
            }

            var fullPath = GetSafeUploadPath(file.StorageKey);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Physical file for ID '{fileId}' was not found.");
            }

            var text = await ExtractTextAsync(file, fullPath, ct);

            await _fileChunkRepository.DeleteByFileIdAsync(fileId, ct);

            var chunks = SplitIntoChunks(text)
                .Select((content, index) => new FileChunk
                {
                    Id = Guid.NewGuid(),
                    FileId = fileId,
                    ChunkIndex = index,
                    Content = content
                })
                .ToList();

            await _fileChunkRepository.AddRangeAsync(chunks, ct);

            await _fileChunkRepository.SaveChangesAsync(ct);
        }

        private static async Task<string> ExtractTextAsync(
            UploadedFile file,
            string fullPath,
            CancellationToken ct)
        {
            var extension = Path.GetExtension(file.Filename);

            if (file.ContentType == "text/plain" ||
                extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
            {
                return await File.ReadAllTextAsync(fullPath, Encoding.UTF8, ct);
            }

            throw new NotSupportedException($"Text extraction for '{file.ContentType}' is not implemented yet.");
        }

        private static List<string> SplitIntoChunks(string text)
        {
            var chunks = new List<string>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return chunks;
            }

            for (var i = 0; i < text.Length; i += ChunkSize)
            {
                var length = Math.Min(ChunkSize, text.Length - i);
                chunks.Add(text.Substring(i, length));
            }

            return chunks;
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
    }
}