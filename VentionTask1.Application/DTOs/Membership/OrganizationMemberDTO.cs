using VentionTask1.Domain.Constants;

namespace VentionTask1.Application.DTOs.Membership
{
    public class OrganizationMemberDTO
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public RoleType Role { get; set; }
    }
}
