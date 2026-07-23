namespace VentionTask1.Application.DTOs
{
    public class UpdateUserDTO
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? NewPassword { get; set; }
        public string? RepeatPassword { get; set; }
        public Guid? OrganizationId { get; set; }
    }
}
