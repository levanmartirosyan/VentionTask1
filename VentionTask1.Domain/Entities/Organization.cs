namespace VentionTask1.Domain.Entities
{
    public class Organization : BaseEntity
    {
        public required string Name { get; set; }
        public ICollection<OrganizationMember> Members { get; set; } = [];
        public ICollection<UploadedFile> Files { get; set; } = [];
        public ICollection<ChatSession> ChatSessions { get; set; } = [];
    }
}
