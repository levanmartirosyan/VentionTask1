using VentionTask1.Application.DTOs;
using VentionTask1.WebApi.GraphQL.DataLoaders;

namespace VentionTask1.WebApi.GraphQL.Types
{
    [ExtendObjectType(typeof(UserDTO))]
    public class UserType
    {
        public async Task<OrganizationDTO?> GetOrganization(
            [Parent] UserDTO user,
            OrganizationByIdDataLoader organizationLoader,
            CancellationToken ct)
        {
            if (!user.OrganizationId.HasValue)
            {
                return null;
            }

            return await organizationLoader.LoadAsync(
                user.OrganizationId.Value,
                ct);
        }
    }
}
