using FluentValidation;
using VentionTask1.Application.DTOs;
using VentionTask1.Application.Extensions;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;

namespace VentionTask1.Application.Services.Implementation
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IValidator<CreateOrganizationDTO> _createOrganizationValidator;
        private readonly IValidator<UpdateOrganizationDTO> _updateOrganizationValidator;

        public OrganizationService(
            IOrganizationRepository organizationRepository, 
            IValidator<CreateOrganizationDTO> createOrganizationValidator, 
            IValidator<UpdateOrganizationDTO> updateOrganizationValidator
            )
        {
            _organizationRepository = organizationRepository;
            _createOrganizationValidator = createOrganizationValidator;
            _updateOrganizationValidator = updateOrganizationValidator;
        }

        public async Task<PaginatedResponseDTO<OrganizationDTO>> GetOrganizationsPaginatedAsync(Guid? cursor, int pageSize, CancellationToken ct)
        {
            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            var organizations = await _organizationRepository.GetOrganizationsPaginatedAsync(cursor, pageSize, ct);

            var hasNextPage = organizations.Count > pageSize;

            var items = organizations
                .Take(pageSize)
                .Select(organization => organization.ToDto())
                .ToList();

            return new PaginatedResponseDTO<OrganizationDTO>
            {
                Items = items,
                HasNextPage = hasNextPage,
                NextCursor = hasNextPage && items.Any()
                    ? items.Last().Id
                    : null
            };
        }

        public async Task<OrganizationDTO?> GetOrganizationByIdAsync(Guid id, CancellationToken ct)
        {
            var organization = await _organizationRepository.GetOrganizationByIdAsync(id, ct);

            if (organization == null)
            {
                throw new KeyNotFoundException($"Organization with ID '{id}' not found.");
            }

            return organization.ToDto();
        }

        public async Task<OrganizationDTO> CreateOrganizationAsync(CreateOrganizationDTO organizationDTO, CancellationToken ct)
        {
            var validationResult = await _createOrganizationValidator.ValidateAsync(organizationDTO);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingOrganization = await _organizationRepository.GetOrganizationByNameAsync(organizationDTO.Name, ct);

            if (existingOrganization != null)
            {
                throw new InvalidOperationException($"An organization with the name '{organizationDTO.Name}' already exists.");
            }

            var createdOrganization = await _organizationRepository.CreateOrganizationAsync(organizationDTO.ToEntity(), ct);

            if (!await _organizationRepository.SaveChangesAsync(ct))
            {
                throw new InvalidOperationException("Internal server error occurred while saving changes.");
            }

            return createdOrganization.ToDto();
        }

        public async Task<OrganizationDTO> UpdateOrganizationAsync(Guid id, UpdateOrganizationDTO organizationDTO, CancellationToken ct)
        {
            var validationResult = await _updateOrganizationValidator.ValidateAsync(organizationDTO);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var organization = await _organizationRepository.GetOrganizationByIdAsync(id, ct);

            if (organization == null)
            {
                throw new KeyNotFoundException($"Organization with ID '{id}' not found.");
            }

            var existingOrganization = await _organizationRepository.GetOrganizationByNameAsync(organizationDTO.Name, ct);

            if (existingOrganization != null && existingOrganization.Id != organization.Id)
            {
                throw new InvalidOperationException($"An organization with the name '{organizationDTO.Name}' already exists.");
            }

            organization.Name = organizationDTO.Name;

            await _organizationRepository.UpdateOrganizationAsync(organization);

            if (!await _organizationRepository.SaveChangesAsync(ct))
            {
                throw new InvalidOperationException("Internal server error occurred while saving changes.");
            }

            return organization.ToDto();
        }

        public async Task DeleteOrganizationAsync(Guid id, CancellationToken ct)
        {
            var organization = await _organizationRepository.GetOrganizationByIdAsync(id, ct);

            if (organization == null)
            {
                throw new KeyNotFoundException($"Organization with ID '{id}' was not found.");
            }

            await _organizationRepository.DeleteOrganizationAsync(organization);

            if (!await _organizationRepository.SaveChangesAsync(ct))
            {
                throw new InvalidOperationException("Internal server error occurred while saving changes.");
            }
        }
    }
}
