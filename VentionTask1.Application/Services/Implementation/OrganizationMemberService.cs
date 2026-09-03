using FluentValidation;
using VentionTask1.Application.DTOs;
using VentionTask1.Application.DTOs.Membership;
using VentionTask1.Application.Extensions;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Constants;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Services.Implementation
{
    public class OrganizationMemberService : IOrganizationMemberService
    {
        private readonly IOrganizationMemberRepository _memberRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IValidator<AddOrganizationMemberDTO> _addMemberValidator;
        private readonly IValidator<UpdateOrganizationMemberRoleDTO> _updateRoleValidator;

        public OrganizationMemberService(
            IOrganizationMemberRepository memberRepository,
            IOrganizationRepository organizationRepository,
            IUserRepository userRepository,
            IValidator<AddOrganizationMemberDTO> addMemberValidator,
            IValidator<UpdateOrganizationMemberRoleDTO> updateRoleValidator)
        {
            _memberRepository = memberRepository;
            _organizationRepository = organizationRepository;
            _userRepository = userRepository;
            _addMemberValidator = addMemberValidator;
            _updateRoleValidator = updateRoleValidator;
        }

        public async Task<PaginatedResponseDTO<OrganizationMemberDTO>> GetMembersAsync(Guid organizationId, Guid? cursor, int pageSize, CancellationToken ct)
        {
            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            var organization = await _organizationRepository.GetOrganizationByIdAsync(organizationId, ct);

            if (organization == null)
            {
                throw new KeyNotFoundException($"Organization with ID '{organizationId}' was not found.");
            }

            var members = await _memberRepository.GetMembersPaginatedAsync(
                organizationId,
                cursor,
                pageSize,
                ct);

            var hasNextPage = members.Count > pageSize;

            var items = members
                .Take(pageSize)
                .Select(member => member.ToDto())
                .ToList();

            return new PaginatedResponseDTO<OrganizationMemberDTO>
            {
                Items = items,
                HasNextPage = hasNextPage,
                NextCursor = hasNextPage && members.Any()
                    ? members.Take(pageSize).Last().Id
                    : null
            };
        }

        public async Task<OrganizationMemberDTO> AddMemberAsync(Guid organizationId, AddOrganizationMemberDTO dto, CancellationToken ct)
        {
            var validationResult = await _addMemberValidator.ValidateAsync(dto, ct);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var organization = await _organizationRepository.GetOrganizationByIdAsync(organizationId, ct);

            if (organization == null)
            {
                throw new KeyNotFoundException($"Organization with ID '{organizationId}' was not found.");
            }

            var user = await _userRepository.GetUserByEmailAsync(dto.Email, ct);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with email '{dto.Email}' was not found.");
            }

            var existingMember = await _memberRepository.GetByOrganizationAndUserAsync(organizationId, user.Id, ct);

            if (existingMember != null)
            {
                throw new InvalidOperationException("User is already a member of this organization.");
            }

            var member = new OrganizationMember
            {
                OrganizationId = organizationId,
                UserId = user.Id,
                Role = dto.Role == default ? RoleType.MEMBER : dto.Role
            };

            await _memberRepository.AddAsync(member, ct);

            if (!await _memberRepository.SaveChangesAsync(ct))
            {
                throw new InvalidOperationException("Organization member could not be created.");
            }

            member.User = user;
            member.Organization = organization;

            return member.ToDto();
        }

        public async Task<OrganizationMemberDTO> UpdateMemberRoleAsync(Guid organizationId, Guid userId, UpdateOrganizationMemberRoleDTO dto, CancellationToken ct)
        {
            var validationResult = await _updateRoleValidator.ValidateAsync(dto, ct);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var member = await _memberRepository.GetByOrganizationAndUserAsync(organizationId, userId, ct);

            if (member == null)
            {
                throw new KeyNotFoundException("Organization member was not found.");
            }

            member.Role = dto.Role;

            if (!await _memberRepository.SaveChangesAsync(ct))
            {
                throw new InvalidOperationException("Organization member role could not be updated.");
            }

            return member.ToDto();
        }

        public async Task RemoveMemberAsync(Guid organizationId, Guid userId, CancellationToken ct)
        {
            var member = await _memberRepository.GetByOrganizationAndUserAsync(organizationId, userId, ct);

            if (member == null)
            {
                throw new KeyNotFoundException("Organization member was not found.");
            }

            await _memberRepository.DeleteAsync(member);

            if (!await _memberRepository.SaveChangesAsync(ct))
            {
                throw new InvalidOperationException("Organization member could not be removed.");
            }
        }
    }
}
