namespace VentionTask1.Application.DTOs
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required Guid OrganizationId { get; set; }
        public required string OrganizationName { get; set; }
    }
}
