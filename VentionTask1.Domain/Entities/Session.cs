namespace VentionTask1.Domain.Entities
{
    public class Session : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public required string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
    }
}
