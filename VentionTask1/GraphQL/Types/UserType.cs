using VentionTask1.Application.DTOs;
using VentionTask1.Application.DTOs.Membership;

namespace VentionTask1.WebApi.GraphQL.Types
{
    [ExtendObjectType(typeof(UserDTO))]
    public class UserType
    {
        public IEnumerable<UserOrganizationMembershipDTO> GetOrganisations(
            [Parent] UserDTO user)
        {
            return user.Organisations;
        }
    }
}