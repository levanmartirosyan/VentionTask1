using VentionTask1.Domain.Constants;

namespace VentionTask1.Application.Services.Interfaces
{
    public interface IOrganizationPermissionService
    {
        void EnsurePlatformAdminOrOwner(RoleType platformRole);

        void EnsureCanReadUser(Guid currentUserId, RoleType platformRole, Guid targetUserId);

        Task EnsureCanAccessOrganizationAsync(
            Guid userId,
            RoleType platformRole,
            Guid organizationId,
            CancellationToken ct);

        Task EnsureCanManageOrganizationAsync(
            Guid userId,
            RoleType platformRole,
            Guid organizationId,
            CancellationToken ct);

        Task EnsureCanViewMembersAsync(
            Guid userId,
            RoleType platformRole,
            Guid organizationId,
            CancellationToken ct);

        Task EnsureCanManageMembersAsync(
            Guid userId,
            RoleType platformRole,
            Guid organizationId,
            CancellationToken ct);

        Task EnsureCanViewFilesAsync(
            Guid userId,
            RoleType platformRole,
            Guid? organizationId,
            CancellationToken ct);

        Task EnsureCanUploadFileAsync(
            Guid userId,
            RoleType platformRole,
            Guid? organizationId,
            CancellationToken ct);

        Task EnsureCanManageFileAsync(
            Guid userId,
            RoleType platformRole,
            Guid fileId,
            CancellationToken ct);
    }
}
