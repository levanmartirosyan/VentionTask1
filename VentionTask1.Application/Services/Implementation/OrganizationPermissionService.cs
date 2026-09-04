using VentionTask1.Application.Exceptions;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Constants;

namespace VentionTask1.Application.Services.Implementation
{
    public class OrganizationPermissionService : IOrganizationPermissionService
    {
        private readonly IOrganizationMemberRepository _memberRepository;
        private readonly IFileRepository _fileRepository;

        public OrganizationPermissionService(
            IOrganizationMemberRepository memberRepository,
            IFileRepository fileRepository)
        {
            _memberRepository = memberRepository;
            _fileRepository = fileRepository;
        }

        public void EnsurePlatformAdminOrOwner(RoleType platformRole)
        {
            if (!HasGlobalAccess(platformRole))
            {
                throw new ForbiddenAccessException("You do not have permission to perform this action.");
            }
        }

        public void EnsureCanReadUser(Guid currentUserId, RoleType platformRole, Guid targetUserId)
        {
            if (HasGlobalAccess(platformRole) || currentUserId == targetUserId)
            {
                return;
            }

            throw new ForbiddenAccessException("You do not have permission to access this user.");
        }

        public async Task EnsureCanAccessOrganizationAsync(
            Guid userId,
            RoleType platformRole,
            Guid organizationId,
            CancellationToken ct)
        {
            if (HasGlobalAccess(platformRole))
            {
                return;
            }

            var member = await _memberRepository.GetByOrganizationAndUserAsync(organizationId, userId, ct);

            if (member == null)
            {
                throw new ForbiddenAccessException("You do not have access to this organization.");
            }
        }

        public async Task EnsureCanManageOrganizationAsync(
            Guid userId,
            RoleType platformRole,
            Guid organizationId,
            CancellationToken ct)
        {
            if (HasGlobalAccess(platformRole))
            {
                return;
            }

            var member = await _memberRepository.GetByOrganizationAndUserAsync(organizationId, userId, ct);

            if (member == null || !CanManage(member.Role))
            {
                throw new ForbiddenAccessException("You do not have permission to manage this organization.");
            }
        }

        public async Task EnsureCanViewMembersAsync(
            Guid userId,
            RoleType platformRole,
            Guid organizationId,
            CancellationToken ct)
        {
            await EnsureCanAccessOrganizationAsync(userId, platformRole, organizationId, ct);
        }

        public async Task EnsureCanManageMembersAsync(
            Guid userId,
            RoleType platformRole,
            Guid organizationId,
            CancellationToken ct)
        {
            if (HasGlobalAccess(platformRole))
            {
                return;
            }

            var member = await _memberRepository.GetByOrganizationAndUserAsync(organizationId, userId, ct);

            if (member == null || !CanManage(member.Role))
            {
                throw new ForbiddenAccessException("You do not have permission to manage organization members.");
            }
        }

        public async Task EnsureCanViewFilesAsync(
            Guid userId,
            RoleType platformRole,
            Guid? organizationId,
            CancellationToken ct)
        {
            if (HasGlobalAccess(platformRole))
            {
                return;
            }

            if (!organizationId.HasValue)
            {
                throw new ForbiddenAccessException("Organization id is required.");
            }

            await EnsureCanAccessOrganizationAsync(userId, platformRole, organizationId.Value, ct);
        }

        public async Task EnsureCanUploadFileAsync(
            Guid userId,
            RoleType platformRole,
            Guid? organizationId,
            CancellationToken ct)
        {
            await EnsureCanViewFilesAsync(userId, platformRole, organizationId, ct);
        }

        public async Task EnsureCanManageFileAsync(
            Guid userId,
            RoleType platformRole,
            Guid fileId,
            CancellationToken ct)
        {
            if (HasGlobalAccess(platformRole))
            {
                return;
            }

            var file = await _fileRepository.GetByIdAsync(fileId, ct);

            if (file == null)
            {
                throw new KeyNotFoundException($"File with ID '{fileId}' was not found.");
            }

            if (!file.OrganizationId.HasValue)
            {
                throw new ForbiddenAccessException("You do not have permission to manage this file.");
            }

            await EnsureCanManageOrganizationAsync(userId, platformRole, file.OrganizationId.Value, ct);
        }

        private static bool HasGlobalAccess(RoleType platformRole)
        {
            return platformRole is RoleType.ADMIN or RoleType.OWNER;
        }

        private static bool CanManage(RoleType organizationRole)
        {
            return organizationRole is RoleType.ADMIN or RoleType.OWNER;
        }
    }
}
