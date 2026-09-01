namespace VentionTask1.Application.DTOs
{
    public class CreateSessionDTO
    {
        public Guid UserId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
