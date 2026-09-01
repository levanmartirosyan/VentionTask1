using FluentValidation;
using FluentValidation.Results;
using Moq;
using VentionTask1.Application.DTOs;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Implementation;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Constants;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IValidator<CreateUserDTO>> _createValidatorMock;
        private readonly Mock<IValidator<UpdateUserDTO>> _updateValidatorMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly Mock<IOrganizationRepository> _organizationRepositoryMock;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _createValidatorMock = new Mock<IValidator<CreateUserDTO>>();
            _updateValidatorMock = new Mock<IValidator<UpdateUserDTO>>();
            _passwordServiceMock = new Mock<IPasswordService>();
            _organizationRepositoryMock = new Mock<IOrganizationRepository>();

            _createValidatorMock
                .Setup(validator => validator.ValidateAsync(It.IsAny<CreateUserDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _updateValidatorMock
                .Setup(validator => validator.ValidateAsync(It.IsAny<UpdateUserDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _service = new UserService(
                _userRepositoryMock.Object,
                _createValidatorMock.Object,
                _updateValidatorMock.Object,
                _passwordServiceMock.Object,
                _organizationRepositoryMock.Object);
        }

        [Fact]
        public async Task GetUsersPaginatedAsync_WhenRepositoryReturnsMoreThanPageSize_ShouldReturnPageAndNextCursor()
        {
            var users = new List<User>
            {
                CreateUser("User 1", "user1@test.com"),
                CreateUser("User 2", "user2@test.com"),
                CreateUser("User 3", "user3@test.com")
            };

            _userRepositoryMock
                .Setup(repository => repository.GetUsersPaginatedAsync(null, 2, CancellationToken.None))
                .ReturnsAsync(users);

            var result = await _service.GetUsersPaginatedAsync(null, 2, CancellationToken.None);

            Assert.Equal(2, result.Items.Count);
            Assert.True(result.HasNextPage);
            Assert.Equal(users[1].Id, result.NextCursor);
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenUserDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            var userId = Guid.NewGuid();

            _userRepositoryMock
                .Setup(repository => repository.GetUserByIdAsync(userId, CancellationToken.None))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.GetUserByIdAsync(userId, CancellationToken.None));
        }

        [Fact]
        public async Task CreateUserAsync_WhenEmailAlreadyExists_ShouldThrowInvalidOperationException()
        {
            var dto = new CreateUserDTO
            {
                Name = "Test User",
                Email = "test@gmail.com",
                Password = "Password123!"
            };

            _userRepositoryMock
                .Setup(repository => repository.GetUserByEmailAsync(dto.Email, CancellationToken.None))
                .ReturnsAsync(CreateUser(dto.Name, dto.Email));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateUserAsync(dto, CancellationToken.None));
        }

        [Fact]
        public async Task CreateUserAsync_WhenDataIsValid_ShouldCreateMemberWithHashedPassword()
        {
            var dto = new CreateUserDTO
            {
                Name = "Test User",
                Email = "test@gmail.com",
                Password = "Password123!"
            };

            _userRepositoryMock
                .Setup(repository => repository.GetUserByEmailAsync(dto.Email, CancellationToken.None))
                .ReturnsAsync((User?)null);

            _passwordServiceMock
                .Setup(service => service.HashPassword(It.IsAny<User>(), dto.Password))
                .Returns("hashed-password");

            _userRepositoryMock
                .Setup(repository => repository.CreateUserAsync(It.IsAny<User>(), CancellationToken.None))
                .ReturnsAsync((User user, CancellationToken _) =>
                {
                    user.Id = Guid.NewGuid();
                    return user;
                });

            _userRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            var result = await _service.CreateUserAsync(dto, CancellationToken.None);

            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.Email, result.Email);
            Assert.Equal(RoleType.MEMBER, result.Role);
            _passwordServiceMock.Verify(service => service.HashPassword(It.IsAny<User>(), dto.Password), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_WhenUserExists_ShouldDeleteUser()
        {
            var userId = Guid.NewGuid();
            var user = CreateUser("Test User", "test@gmail.com");
            user.Id = userId;

            _userRepositoryMock
                .Setup(repository => repository.GetUserByIdAsync(userId, CancellationToken.None))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            await _service.DeleteUserAsync(userId, CancellationToken.None);

            _userRepositoryMock.Verify(repository => repository.DeleteUserAsync(user), Times.Once);
        }

        private static User CreateUser(string name, string email)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                PasswordHash = "hashed-password",
                Role = RoleType.MEMBER
            };
        }
    }
}
