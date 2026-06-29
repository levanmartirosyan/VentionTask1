namespace VentionTask1.Domain.Entities
{
    public class User : BaseEntity
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;
        public ICollection<Session> Sessions { get; set; } = [];
    }
}
