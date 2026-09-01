using Moq;
using VentionTask1.Application.DTOs;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Implementation;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Constants;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly Mock<ISessionRepository> _sessionRepositoryMock;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordServiceMock = new Mock<IPasswordService>();
            _sessionRepositoryMock = new Mock<ISessionRepository>();
            _service = new AuthService(_userRepositoryMock.Object, _passwordServiceMock.Object, _sessionRepositoryMock.Object);
        }

        [Fact]
        public async Task ValidateLoginAsync_WhenUserDoesNotExist_ShouldThrowUnauthorizedAccessException()
        {
            var request = new LoginRequestDTO
            {
                Email = "test@gmail.com",
                Password = "Password123!"
            };

            _userRepositoryMock
                .Setup(repository => repository.GetUserByEmailAsync(request.Email, CancellationToken.None))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.ValidateLoginAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task ValidateLoginAsync_WhenPasswordIsInvalid_ShouldThrowUnauthorizedAccessException()
        {
            var request = new LoginRequestDTO
            {
                Email = "test@gmail.com",
                Password = "WrongPassword123!"
            };
            var user = CreateUser(request.Email);

            _userRepositoryMock
                .Setup(repository => repository.GetUserByEmailAsync(request.Email, CancellationToken.None))
                .ReturnsAsync(user);

            _passwordServiceMock
                .Setup(service => service.VerifyPassword(user, user.PasswordHash, request.Password))
                .Returns(false);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.ValidateLoginAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task ValidateLoginAsync_WhenCredentialsAreValid_ShouldReturnUser()
        {
            var request = new LoginRequestDTO
            {
                Email = "test@gmail.com",
                Password = "Password123!"
            };
            var user = CreateUser(request.Email);

            _userRepositoryMock
                .Setup(repository => repository.GetUserByEmailAsync(request.Email, CancellationToken.None))
                .ReturnsAsync(user);

            _passwordServiceMock
                .Setup(service => service.VerifyPassword(user, user.PasswordHash, request.Password))
                .Returns(true);

            var result = await _service.ValidateLoginAsync(request, CancellationToken.None);

            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Email, result.Email);
        }

        [Fact]
        public async Task CreateSessionAsync_WhenSessionIsCreated_ShouldSaveSession()
        {
            var request = new CreateSessionDTO
            {
                UserId = Guid.NewGuid(),
                IpAddress = "127.0.0.1",
                UserAgent = "Test Browser"
            };

            _sessionRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            await _service.CreateSessionAsync(request, CancellationToken.None);

            _sessionRepositoryMock.Verify(
                repository => repository.AddAsync(
                    It.Is<Session>(session =>
                        session.UserId == request.UserId &&
                        session.IpAddress == request.IpAddress &&
                        session.UserAgent == request.UserAgent &&
                        session.IsActive &&
                        session.LoggedOutAt == null),
                    CancellationToken.None),
                Times.Once);

            _sessionRepositoryMock.Verify(
                repository => repository.SaveChangesAsync(CancellationToken.None),
                Times.Once);
        }

        private static User CreateUser(string email)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = email,
                PasswordHash = "hashed-password",
                Role = RoleType.MEMBER
            };
        }
    }
}
