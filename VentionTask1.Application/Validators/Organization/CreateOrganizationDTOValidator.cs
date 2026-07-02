using FluentValidation;
using VentionTask1.Application.DTOs;

namespace VentionTask1.Application.Validators.Organization
{
    public class CreateOrganizationDTOValidator : AbstractValidator<CreateOrganizationDTO>
    {
        public CreateOrganizationDTOValidator()
        {
            RuleFor(x => x.Name)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Organization name is required.")
                .MaximumLength(100)
                .WithMessage("Organization name must not exceed 100 characters.");
        }
    }
}
