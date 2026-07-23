using VentionTask1.Domain.Constants;

namespace VentionTask1.Domain.Entities
{
    public class User : BaseEntity
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public required RoleType Role { get; set; }
        public Guid? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
        public ICollection<Session> Sessions { get; set; } = [];
        public ICollection<UploadedFile> Files { get; set; } = [];
    }
}
