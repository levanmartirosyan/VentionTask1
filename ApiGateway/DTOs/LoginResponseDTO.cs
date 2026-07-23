namespace ApiGateway.DTOs
{
    public class LoginResponseDTO
    {
        public required Guid Id { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string Role { get; set; }
        public string? Image { get; set; }
        public required string AccessToken { get; set; }
    }
}
