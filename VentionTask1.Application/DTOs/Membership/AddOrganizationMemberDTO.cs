using VentionTask1.Domain.Constants;

namespace VentionTask1.Application.DTOs.Membership
{
    public class AddOrganizationMemberDTO
    {
        public required string Email { get; set; }
        public RoleType Role { get; set; } = RoleType.MEMBER;
    }
}
