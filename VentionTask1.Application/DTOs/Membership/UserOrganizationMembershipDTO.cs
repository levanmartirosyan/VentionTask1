using VentionTask1.Domain.Constants;

namespace VentionTask1.Application.DTOs.Membership
{
    public class UserOrganizationMembershipDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public RoleType Role { get; set; }
    }
}
