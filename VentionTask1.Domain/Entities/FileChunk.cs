namespace VentionTask1.Domain.Entities
{
    public class FileChunk : BaseEntity
    {
        public Guid FileId { get; set; }
        public UploadedFile File { get; set; } = null!;

        public int ChunkIndex { get; set; }
        public required string Content { get; set; }
    }
}
