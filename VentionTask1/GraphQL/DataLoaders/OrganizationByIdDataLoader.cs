using VentionTask1.Application.DTOs;
using VentionTask1.Application.Extensions;
using VentionTask1.Application.Repositories.Interfaces;

namespace VentionTask1.WebApi.GraphQL.DataLoaders
{
    public sealed class OrganizationByIdDataLoader
          : BatchDataLoader<Guid, OrganizationDTO>
    {
        private readonly IOrganizationRepository _organizationRepository;

        public OrganizationByIdDataLoader(
            IBatchScheduler batchScheduler,
            DataLoaderOptions options,
            IOrganizationRepository organizationRepository)
            : base(batchScheduler, options)
        {
            _organizationRepository = organizationRepository;
        }

        protected override async Task<IReadOnlyDictionary<Guid, OrganizationDTO>> LoadBatchAsync(
            IReadOnlyList<Guid> keys,
            CancellationToken ct)
        {
            var organizations = await _organizationRepository
                .GetOrganizationsByIdsAsync(keys, ct);

            return organizations.ToDictionary(
                org => org.Id,
                org => org.ToDto());
        }
    }
}
