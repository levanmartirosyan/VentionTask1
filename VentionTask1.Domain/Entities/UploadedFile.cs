namespace VentionTask1.Domain.Entities
{
    public class UploadedFile : BaseEntity
    {
        public required string Filename { get; set; }
        public required string StoredFileName { get; set; }
        public long Size { get; set; }
        public required string Status { get; set; }
        public required string ContentType { get; set; }
        public required string Checksum { get; set; }
        public required string StorageKey { get; set; }

        public Guid? OrganizationId { get; set; }
        public Organization? Organization { get; set; }

        public Guid? OwnerId { get; set; }
        public User? Owner { get; set; }

        public string? Application { get; set; }
        public string? ProcessingError { get; set; }
    }
}
