using FluentValidation;
using System.Data.Common;
using System.Transactions;
using VentionTask1.Application.DTOs;
using VentionTask1.Application.Extensions;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Entities;

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

        public async Task<List<OrganizationDTO>> GetAllOrganizationsAsync(CancellationToken ct)
        {
            var organizations = await _organizationRepository.GetAllOrganizationsAsync(ct);

            return organizations.Select(org => org.ToDto()).ToList();
        }

        public async Task<OrganizationDTO?> GetOrganizationByIdAsync(Guid id, CancellationToken ct)
        {
            var organization = await _organizationRepository.GetOrganizationByIdAsync(id, ct);

            if (organization == null) return null;

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

            if (existingOrganization != null)
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

        public async Task<bool> DeleteOrganizationAsync(Guid id, CancellationToken ct)
        {
            var organization = await _organizationRepository.GetOrganizationByIdAsync(id, ct);

            if (organization == null)
            {
                return false;
            }

            await _organizationRepository.DeleteOrganizationAsync(organization);

            if (!await _organizationRepository.SaveChangesAsync(ct))
            {
                throw new InvalidOperationException("Internal server error occurred while saving changes.");
            }

            return true;
        }

        // example of transaction
        //await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        //try
        //{
        // multiple database operations

        //    await _dbContext.SaveChangesAsync(ct);
        //    await transaction.CommitAsync(ct);
        //}
        //catch
        //{
        //    await transaction.RollbackAsync(ct);
        //    throw;
        //}
    }
}
