using VentionTask1.Application.DTOs;
using VentionTask1.Application.Validators.User;

namespace VentionTask1.Tests.Validators
{
    public class UserValidatorTests
    {
        [Fact]
        public void CreateUserValidator_WhenDataIsValid_ShouldBeValid()
        {
            var validator = new CreateUserDTOValidator();
            var dto = new CreateUserDTO
            {
                Name = "Test User",
                Email = "test@gmail.com",
                Password = "Password123!"
            };

            var result = validator.Validate(dto);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void CreateUserValidator_WhenEmailIsInvalid_ShouldBeInvalid()
        {
            var validator = new CreateUserDTOValidator();
            var dto = new CreateUserDTO
            {
                Name = "Test User",
                Email = "wrong-email",
                Password = "Password123!"
            };

            var result = validator.Validate(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void CreateUserValidator_WhenPasswordHasNoUppercase_ShouldBeInvalid()
        {
            var validator = new CreateUserDTOValidator();
            var dto = new CreateUserDTO
            {
                Name = "Test User",
                Email = "test@gmail.com",
                Password = "password123!"
            };

            var result = validator.Validate(dto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.ErrorMessage == "Password must contain at least one uppercase letter.");
        }

        [Fact]
        public void CreateUserValidator_WhenPasswordHasNoSymbol_ShouldBeInvalid()
        {
            var validator = new CreateUserDTOValidator();
            var dto = new CreateUserDTO
            {
                Name = "Test User",
                Email = "test@gmail.com",
                Password = "Password123"
            };

            var result = validator.Validate(dto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.ErrorMessage == "Password must contain at least one symbol.");
        }

        [Fact]
        public void UpdateUserValidator_WhenRepeatPasswordDoesNotMatch_ShouldBeInvalid()
        {
            var validator = new UpdateUserDTOValidator();
            var dto = new UpdateUserDTO
            {
                NewPassword = "Password123!",
                RepeatPassword = "Password456!"
            };

            var result = validator.Validate(dto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.ErrorMessage == "Repeat password must match new password.");
        }

        [Fact]
        public void UpdateUserValidator_WhenRepeatPasswordProvidedWithoutNewPassword_ShouldBeInvalid()
        {
            var validator = new UpdateUserDTOValidator();
            var dto = new UpdateUserDTO
            {
                RepeatPassword = "Password123!"
            };

            var result = validator.Validate(dto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.ErrorMessage == "New password is required when repeat password is provided.");
        }
    }
}
