using VentionTask1.Application.DTOs;
using VentionTask1.Application.Services.Interfaces;

namespace VentionTask1.WebApi.GraphQL.Queries
{
    [ExtendObjectType("Query")]
    public class OrganizationQuery
    {
        public Task<PaginatedResponseDTO<OrganizationDTO>> GetOrganizations(
            [Service] IOrganizationService organizationService,
            Guid? cursor,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            return organizationService.GetOrganizationsPaginatedAsync(
                cursor,
                pageSize,
                ct);
        }
    }
}
