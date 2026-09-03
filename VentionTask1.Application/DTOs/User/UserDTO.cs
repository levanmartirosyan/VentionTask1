using VentionTask1.Application.DTOs.Membership;
using VentionTask1.Domain.Constants;

namespace VentionTask1.Application.DTOs
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required RoleType Role { get; set; }
        public List<UserOrganizationMembershipDTO> Organisations { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
