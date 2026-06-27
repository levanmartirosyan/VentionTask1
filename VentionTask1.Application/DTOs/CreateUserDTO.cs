namespace VentionTask1.Application.DTOs
{
    public class CreateUserDTO
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password{ get; set; }
        public required Guid OrganizationId { get; set; }
    }
}
