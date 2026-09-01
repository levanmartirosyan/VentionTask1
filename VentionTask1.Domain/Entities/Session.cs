namespace VentionTask1.Domain.Entities
{
    public class Session : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime LoggedInAt { get; set; }
        public DateTime? LoggedOutAt { get; set; }

        public bool IsActive { get; set; }

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
