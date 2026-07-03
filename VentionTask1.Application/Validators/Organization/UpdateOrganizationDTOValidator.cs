using FluentValidation;
using VentionTask1.Application.DTOs;

namespace VentionTask1.Application.Validators.Organization
{
    public class UpdateOrganizationDTOValidator : AbstractValidator<UpdateOrganizationDTO>
    {
        public UpdateOrganizationDTOValidator()
        {
            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Organization name is required.")
                .Must(name => name == name.Trim())
                .WithMessage("Organization name must not contain leading or trailing spaces.")
                .MaximumLength(100)
                .WithMessage("Organization name must not exceed 100 characters.");
        }
    }
}
