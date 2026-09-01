using VentionTask1.Application.DTOs;
using VentionTask1.Application.Validators.Organization;

namespace VentionTask1.Tests.Validators
{
    public class OrganizationValidatorTests
    {
        [Fact]
        public void CreateOrganizationValidator_WhenNameIsValid_ShouldBeValid()
        {
            var validator = new CreateOrganizationDTOValidator();
            var dto = new CreateOrganizationDTO { Name = "Vention" };

            var result = validator.Validate(dto);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void CreateOrganizationValidator_WhenNameHasSpaces_ShouldBeInvalid()
        {
            var validator = new CreateOrganizationDTOValidator();
            var dto = new CreateOrganizationDTO { Name = " Vention " };

            var result = validator.Validate(dto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.ErrorMessage == "Organization name must not contain leading or trailing spaces.");
        }

        [Fact]
        public void UpdateOrganizationValidator_WhenNameIsEmpty_ShouldBeInvalid()
        {
            var validator = new UpdateOrganizationDTOValidator();
            var dto = new UpdateOrganizationDTO { Name = string.Empty };

            var result = validator.Validate(dto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.ErrorMessage == "Organization name is required.");
        }

        [Fact]
        public void UpdateOrganizationValidator_WhenNameIsTooLong_ShouldBeInvalid()
        {
            var validator = new UpdateOrganizationDTOValidator();
            var dto = new UpdateOrganizationDTO { Name = new string('a', 101) };

            var result = validator.Validate(dto);

            Assert.False(result.IsValid);
        }
    }
}
