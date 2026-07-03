using FluentValidation;
using VentionTask1.Application.DTOs;

namespace VentionTask1.Application.Validators.Organization
{
    public class UpdateOrganizationDTOValidator : AbstractValidator<UpdateOrganizationDTO>
    {
        public UpdateOrganizationDTOValidator()
        {
            RuleFor(x => x.Name)
                .Must(name => name == null || name == name.Trim())
                .WithMessage("Organization name must not contain leading or trailing spaces.")
                .Must(name => name == null || !string.IsNullOrWhiteSpace(name))
                .WithMessage("Organization name is required.")
                .MaximumLength(100)
                .WithMessage("Organization name must not exceed 100 characters.");
        }
    }
}
