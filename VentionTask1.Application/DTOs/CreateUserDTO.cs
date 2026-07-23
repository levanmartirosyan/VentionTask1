namespace VentionTask1.Application.DTOs
{
    public class CreateUserDTO
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password{ get; set; }
    }
}
