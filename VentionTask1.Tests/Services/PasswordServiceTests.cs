using Microsoft.AspNetCore.Identity;
using VentionTask1.Application.Services.Implementation;
using VentionTask1.Domain.Constants;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Tests.Services
{
    public class PasswordServiceTests
    {
        private readonly PasswordService _service;

        public PasswordServiceTests()
        {
            _service = new PasswordService(new PasswordHasher<User>());
        }

        [Fact]
        public void HashPassword_ShouldReturnHashedPasswordDifferentFromPlainPassword()
        {
            var user = CreateUser();
            var password = "Password123!";

            var result = _service.HashPassword(user, password);

            Assert.False(string.IsNullOrWhiteSpace(result));
            Assert.NotEqual(password, result);
        }

        [Fact]
        public void VerifyPassword_WhenPasswordMatchesHash_ShouldReturnTrue()
        {
            var user = CreateUser();
            var password = "Password123!";
            var hash = _service.HashPassword(user, password);

            var result = _service.VerifyPassword(user, hash, password);

            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_WhenPasswordDoesNotMatchHash_ShouldReturnFalse()
        {
            var user = CreateUser();
            var hash = _service.HashPassword(user, "Password123!");

            var result = _service.VerifyPassword(user, hash, "WrongPassword123!");

            Assert.False(result);
        }

        private static User CreateUser()
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = "test@gmail.com",
                PasswordHash = string.Empty,
                Role = RoleType.MEMBER
            };
        }
    }
}
