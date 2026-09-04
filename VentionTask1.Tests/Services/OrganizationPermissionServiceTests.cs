using Moq;
using VentionTask1.Application.Exceptions;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Implementation;
using VentionTask1.Domain.Constants;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Tests.Services
{
    public class OrganizationPermissionServiceTests
    {
        private readonly Mock<IOrganizationMemberRepository> _memberRepositoryMock;
        private readonly Mock<IFileRepository> _fileRepositoryMock;
        private readonly OrganizationPermissionService _service;

        public OrganizationPermissionServiceTests()
        {
            _memberRepositoryMock = new Mock<IOrganizationMemberRepository>();
            _fileRepositoryMock = new Mock<IFileRepository>();

            _service = new OrganizationPermissionService(
                _memberRepositoryMock.Object,
                _fileRepositoryMock.Object);
        }

        [Theory]
        [InlineData(RoleType.ADMIN)]
        [InlineData(RoleType.OWNER)]
        public void EnsurePlatformAdminOrOwner_WhenRoleHasGlobalAccess_ShouldNotThrow(RoleType role)
        {
            _service.EnsurePlatformAdminOrOwner(role);
        }

        [Fact]
        public void EnsurePlatformAdminOrOwner_WhenRoleIsMember_ShouldThrowForbiddenAccessException()
        {
            Assert.Throws<ForbiddenAccessException>(() =>
                _service.EnsurePlatformAdminOrOwner(RoleType.MEMBER));
        }

        [Fact]
        public void EnsureCanReadUser_WhenCurrentUserReadsOwnProfile_ShouldNotThrow()
        {
            var userId = Guid.NewGuid();

            _service.EnsureCanReadUser(userId, RoleType.MEMBER, userId);
        }

        [Fact]
        public void EnsureCanReadUser_WhenMemberReadsAnotherUser_ShouldThrowForbiddenAccessException()
        {
            Assert.Throws<ForbiddenAccessException>(() =>
                _service.EnsureCanReadUser(Guid.NewGuid(), RoleType.MEMBER, Guid.NewGuid()));
        }

        [Fact]
        public async Task EnsureCanAccessOrganizationAsync_WhenPlatformAdmin_ShouldNotQueryMemberships()
        {
            await _service.EnsureCanAccessOrganizationAsync(
                Guid.NewGuid(),
                RoleType.ADMIN,
                Guid.NewGuid(),
                CancellationToken.None);

            _memberRepositoryMock.Verify(
                repository => repository.GetByOrganizationAndUserAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task EnsureCanAccessOrganizationAsync_WhenUserIsOrganizationMember_ShouldNotThrow()
        {
            var userId = Guid.NewGuid();
            var organizationId = Guid.NewGuid();

            _memberRepositoryMock
                .Setup(repository => repository.GetByOrganizationAndUserAsync(organizationId, userId, CancellationToken.None))
                .ReturnsAsync(CreateMember(userId, organizationId, RoleType.MEMBER));

            await _service.EnsureCanAccessOrganizationAsync(
                userId,
                RoleType.MEMBER,
                organizationId,
                CancellationToken.None);
        }

        [Fact]
        public async Task EnsureCanAccessOrganizationAsync_WhenUserIsNotOrganizationMember_ShouldThrowForbiddenAccessException()
        {
            var userId = Guid.NewGuid();
            var organizationId = Guid.NewGuid();

            _memberRepositoryMock
                .Setup(repository => repository.GetByOrganizationAndUserAsync(organizationId, userId, CancellationToken.None))
                .ReturnsAsync((OrganizationMember?)null);

            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                _service.EnsureCanAccessOrganizationAsync(
                    userId,
                    RoleType.MEMBER,
                    organizationId,
                    CancellationToken.None));
        }

        [Theory]
        [InlineData(RoleType.ADMIN)]
        [InlineData(RoleType.OWNER)]
        public async Task EnsureCanManageOrganizationAsync_WhenUserHasManagerOrganizationRole_ShouldNotThrow(RoleType organizationRole)
        {
            var userId = Guid.NewGuid();
            var organizationId = Guid.NewGuid();

            _memberRepositoryMock
                .Setup(repository => repository.GetByOrganizationAndUserAsync(organizationId, userId, CancellationToken.None))
                .ReturnsAsync(CreateMember(userId, organizationId, organizationRole));

            await _service.EnsureCanManageOrganizationAsync(
                userId,
                RoleType.MEMBER,
                organizationId,
                CancellationToken.None);
        }

        [Fact]
        public async Task EnsureCanManageOrganizationAsync_WhenUserHasMemberOrganizationRole_ShouldThrowForbiddenAccessException()
        {
            var userId = Guid.NewGuid();
            var organizationId = Guid.NewGuid();

            _memberRepositoryMock
                .Setup(repository => repository.GetByOrganizationAndUserAsync(organizationId, userId, CancellationToken.None))
                .ReturnsAsync(CreateMember(userId, organizationId, RoleType.MEMBER));

            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                _service.EnsureCanManageOrganizationAsync(
                    userId,
                    RoleType.MEMBER,
                    organizationId,
                    CancellationToken.None));
        }

        [Fact]
        public async Task EnsureCanViewFilesAsync_WhenMemberDoesNotProvideOrganizationId_ShouldThrowForbiddenAccessException()
        {
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                _service.EnsureCanViewFilesAsync(
                    Guid.NewGuid(),
                    RoleType.MEMBER,
                    null,
                    CancellationToken.None));
        }

        [Fact]
        public async Task EnsureCanManageFileAsync_WhenFileBelongsToManagedOrganization_ShouldNotThrow()
        {
            var userId = Guid.NewGuid();
            var organizationId = Guid.NewGuid();
            var file = CreateFile(organizationId);

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(file.Id, CancellationToken.None))
                .ReturnsAsync(file);

            _memberRepositoryMock
                .Setup(repository => repository.GetByOrganizationAndUserAsync(organizationId, userId, CancellationToken.None))
                .ReturnsAsync(CreateMember(userId, organizationId, RoleType.ADMIN));

            await _service.EnsureCanManageFileAsync(
                userId,
                RoleType.MEMBER,
                file.Id,
                CancellationToken.None);
        }

        [Fact]
        public async Task EnsureCanManageFileAsync_WhenFileDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            var fileId = Guid.NewGuid();

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(fileId, CancellationToken.None))
                .ReturnsAsync((UploadedFile?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.EnsureCanManageFileAsync(
                    Guid.NewGuid(),
                    RoleType.MEMBER,
                    fileId,
                    CancellationToken.None));
        }

        [Fact]
        public async Task EnsureCanManageFileAsync_WhenFileHasNoOrganization_ShouldThrowForbiddenAccessException()
        {
            var file = CreateFile(null);

            _fileRepositoryMock
                .Setup(repository => repository.GetByIdAsync(file.Id, CancellationToken.None))
                .ReturnsAsync(file);

            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                _service.EnsureCanManageFileAsync(
                    Guid.NewGuid(),
                    RoleType.MEMBER,
                    file.Id,
                    CancellationToken.None));
        }

        private static OrganizationMember CreateMember(Guid userId, Guid organizationId, RoleType role)
        {
            return new OrganizationMember
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrganizationId = organizationId,
                Role = role
            };
        }

        private static UploadedFile CreateFile(Guid? organizationId)
        {
            return new UploadedFile
            {
                Id = Guid.NewGuid(),
                Filename = "test.txt",
                StoredFileName = "stored-test.txt",
                Size = 100,
                Status = "processing",
                ContentType = "text/plain",
                Checksum = "checksum",
                StorageKey = "files/stored-test.txt",
                OrganizationId = organizationId
            };
        }
    }
}
