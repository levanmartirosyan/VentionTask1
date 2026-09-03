namespace VentionTask1.Application.DTOs
{
    public class FileDTO
    {
        public Guid Id { get; set; }
        public required string Filename { get; set; }
        public long Size { get; set; }
        public required string Status { get; set; }
        public required string ContentType { get; set; }
        public required string Checksum { get; set; }
        public required string StorageKey { get; set; }
        public Guid? OrganisationId { get; set; }
        public Guid? OwnerId { get; set; }
        public string? Application { get; set; }
        public string? ProcessingError { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
