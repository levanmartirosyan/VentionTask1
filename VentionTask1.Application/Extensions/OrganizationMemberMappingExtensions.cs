
using VentionTask1.Application.DTOs.Membership;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Extensions
{
    public static class OrganizationMemberMappingExtensions
    {
        public static OrganizationMemberDTO ToDto(this OrganizationMember member)
        {
            return new OrganizationMemberDTO
            {
                UserId = member.UserId,
                Email = member.User.Email,
                Name = member.User.Name,
                Role = member.Role
            };
        }

        public static UserOrganizationMembershipDTO ToEntity(
            this OrganizationMember member)
        {
            return new UserOrganizationMembershipDTO
            {
                Id = member.OrganizationId,
                Name = member.Organization.Name,
                Role = member.Role
            };
        }
    }
}
