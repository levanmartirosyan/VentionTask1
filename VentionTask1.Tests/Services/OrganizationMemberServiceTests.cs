using FluentValidation;
using FluentValidation.Results;
using Moq;
using VentionTask1.Application.DTOs.Membership;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Implementation;
using VentionTask1.Domain.Constants;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Tests.Services
{
    public class OrganizationMemberServiceTests
    {
        private readonly Mock<IOrganizationMemberRepository> _memberRepositoryMock;
        private readonly Mock<IOrganizationRepository> _organizationRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IValidator<AddOrganizationMemberDTO>> _addMemberValidatorMock;
        private readonly Mock<IValidator<UpdateOrganizationMemberRoleDTO>> _updateRoleValidatorMock;
        private readonly OrganizationMemberService _service;

        public OrganizationMemberServiceTests()
        {
            _memberRepositoryMock = new Mock<IOrganizationMemberRepository>();
            _organizationRepositoryMock = new Mock<IOrganizationRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _addMemberValidatorMock = new Mock<IValidator<AddOrganizationMemberDTO>>();
            _updateRoleValidatorMock = new Mock<IValidator<UpdateOrganizationMemberRoleDTO>>();

            _addMemberValidatorMock
                .Setup(validator => validator.ValidateAsync(It.IsAny<AddOrganizationMemberDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _updateRoleValidatorMock
                .Setup(validator => validator.ValidateAsync(It.IsAny<UpdateOrganizationMemberRoleDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _service = new OrganizationMemberService(
                _memberRepositoryMock.Object,
                _organizationRepositoryMock.Object,
                _userRepositoryMock.Object,
                _addMemberValidatorMock.Object,
                _updateRoleValidatorMock.Object);
        }

        [Fact]
        public async Task GetMembersAsync_WhenOrganizationDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            var organizationId = Guid.NewGuid();

            _organizationRepositoryMock
                .Setup(repository => repository.GetOrganizationByIdAsync(organizationId, CancellationToken.None))
                .ReturnsAsync((Organization?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.GetMembersAsync(organizationId, null, 10, CancellationToken.None));
        }

        [Fact]
        public async Task GetMembersAsync_WhenMoreItemsThanPageSize_ShouldReturnPageAndNextCursor()
        {
            var organization = CreateOrganization();
            var firstMember = CreateMember(organization.Id, RoleType.MEMBER);
            var secondMember = CreateMember(organization.Id, RoleType.ADMIN);
            var extraMember = CreateMember(organization.Id, RoleType.MEMBER);

            _organizationRepositoryMock
                .Setup(repository => repository.GetOrganizationByIdAsync(organization.Id, CancellationToken.None))
                .ReturnsAsync(organization);

            _memberRepositoryMock
                .Setup(repository => repository.GetMembersPaginatedAsync(organization.Id, null, 2, CancellationToken.None))
                .ReturnsAsync([firstMember, secondMember, extraMember]);

            var result = await _service.GetMembersAsync(organization.Id, null, 2, CancellationToken.None);

            Assert.True(result.HasNextPage);
            Assert.Equal(secondMember.Id, result.NextCursor);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(firstMember.UserId, result.Items[0].UserId);
            Assert.Equal(secondMember.UserId, result.Items[1].UserId);
        }

        [Fact]
        public async Task AddMemberAsync_WhenUserExistsAndIsNotMember_ShouldCreateOrganizationMember()
        {
            var organization = CreateOrganization();
            var user = CreateUser();
            var dto = new AddOrganizationMemberDTO
            {
                Email = user.Email,
                Role = RoleType.ADMIN
            };
            OrganizationMember? createdMember = null;

            _organizationRepositoryMock
                .Setup(repository => repository.GetOrganizationByIdAsync(organization.Id, CancellationToken.None))
                .ReturnsAsync(organization);

            _userRepositoryMock
                .Setup(repository => repository.GetUserByEmailAsync(user.Email, CancellationToken.None))
                .ReturnsAsync(user);

            _memberRepositoryMock
                .Setup(repository => repository.GetByOrganizationAndUserAsync(organization.Id, user.Id, CancellationToken.None))
                .ReturnsAsync((OrganizationMember?)null);

            _memberRepositoryMock
                .Setup(repository => repository.AddAsync(It.IsAny<OrganizationMember>(), CancellationToken.None))
                .Callback<OrganizationMember, CancellationToken>((member, _) => createdMember = member)
                .Returns(Task.CompletedTask);

            _memberRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            var result = await _service.AddMemberAsync(organization.Id, dto, CancellationToken.None);

            Assert.NotNull(createdMember);
            Assert.Equal(organization.Id, createdMember.OrganizationId);
            Assert.Equal(user.Id, createdMember.UserId);
            Assert.Equal(RoleType.ADMIN, createdMember.Role);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(RoleType.ADMIN, result.Role);
        }

        [Fact]
        public async Task AddMemberAsync_WhenUserAlreadyMember_ShouldThrowInvalidOperationException()
        {
            var organization = CreateOrganization();
            var user = CreateUser();
            var dto = new AddOrganizationMemberDTO
            {
                Email = user.Email,
                Role = RoleType.MEMBER
            };

            _organizationRepositoryMock
                .Setup(repository => repository.GetOrganizationByIdAsync(organization.Id, CancellationToken.None))
                .ReturnsAsync(organization);

            _userRepositoryMock
                .Setup(repository => repository.GetUserByEmailAsync(user.Email, CancellationToken.None))
                .ReturnsAsync(user);

            _memberRepositoryMock
                .Setup(repository => repository.GetByOrganizationAndUserAsync(organization.Id, user.Id, CancellationToken.None))
                .ReturnsAsync(CreateMember(organization.Id, RoleType.MEMBER, user));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddMemberAsync(organization.Id, dto, CancellationToken.None));
        }

        [Fact]
        public async Task UpdateMemberRoleAsync_WhenMemberExists_ShouldUpdateRole()
        {
            var organization = CreateOrganization();
            var user = CreateUser();
            var member = CreateMember(organization.Id, RoleType.MEMBER, user);
            var dto = new UpdateOrganizationMemberRoleDTO
            {
                Role = RoleType.ADMIN
            };

            _memberRepositoryMock
                .Setup(repository => repository.GetByOrganizationAndUserAsync(organization.Id, user.Id, CancellationToken.None))
                .ReturnsAsync(member);

            _memberRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            var result = await _service.UpdateMemberRoleAsync(
                organization.Id,
                user.Id,
                dto,
                CancellationToken.None);

            Assert.Equal(RoleType.ADMIN, member.Role);
            Assert.Equal(RoleType.ADMIN, result.Role);
            Assert.Equal(user.Id, result.UserId);
        }

        [Fact]
        public async Task RemoveMemberAsync_WhenMemberExists_ShouldDeleteMemberAndSaveChanges()
        {
            var organization = CreateOrganization();
            var user = CreateUser();
            var member = CreateMember(organization.Id, RoleType.MEMBER, user);

            _memberRepositoryMock
                .Setup(repository => repository.GetByOrganizationAndUserAsync(organization.Id, user.Id, CancellationToken.None))
                .ReturnsAsync(member);

            _memberRepositoryMock
                .Setup(repository => repository.DeleteAsync(member))
                .Returns(Task.CompletedTask);

            _memberRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            await _service.RemoveMemberAsync(organization.Id, user.Id, CancellationToken.None);

            _memberRepositoryMock.Verify(
                repository => repository.DeleteAsync(member),
                Times.Once);

            _memberRepositoryMock.Verify(
                repository => repository.SaveChangesAsync(CancellationToken.None),
                Times.Once);
        }

        private static Organization CreateOrganization()
        {
            return new Organization
            {
                Id = Guid.NewGuid(),
                Name = "Test Organization"
            };
        }

        private static User CreateUser()
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = RoleType.MEMBER
            };
        }

        private static OrganizationMember CreateMember(Guid organizationId, RoleType role, User? user = null)
        {
            user ??= CreateUser();

            return new OrganizationMember
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                UserId = user.Id,
                User = user,
                Role = role
            };
        }
    }
}
