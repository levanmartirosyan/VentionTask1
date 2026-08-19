using VentionTask1.Application.DTOs;
using VentionTask1.Application.Services.Interfaces;

namespace VentionTask1.WebApi.GraphQL.Mutations
{
    [ExtendObjectType("Mutation")]
    public class OrganizationMutation
    {
        public Task<OrganizationDTO> CreateOrganization(
            CreateOrganizationDTO input,
            [Service] IOrganizationService organizationService,
            CancellationToken ct)
        {
            return organizationService.CreateOrganizationAsync(input, ct);
        }
    }
}
