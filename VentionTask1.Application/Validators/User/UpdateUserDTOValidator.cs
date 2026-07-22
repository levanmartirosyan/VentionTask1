using FluentValidation;
using VentionTask1.Application.DTOs;

namespace VentionTask1.Application.Validators.User
{
    public class UpdateUserDTOValidator : AbstractValidator<UpdateUserDTO>
    {
        public UpdateUserDTOValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.Name));

            RuleFor(x => x.Email)
                .EmailAddress()
                .MaximumLength(255)
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.NewPassword)
                .MinimumLength(8)
                .MaximumLength(255)
                .When(x => !string.IsNullOrWhiteSpace(x.NewPassword));

            RuleFor(x => x.RepeatPassword)
                .Equal(x => x.NewPassword)
                .WithMessage("Repeat password must match new password.")
                .When(x => !string.IsNullOrWhiteSpace(x.NewPassword));

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .When(x => !string.IsNullOrWhiteSpace(x.RepeatPassword))
                .WithMessage("New password is required when repeat password is provided.");

            RuleFor(x => x.OrganizationId)
                .NotEmpty()
                .When(x => x.OrganizationId.HasValue);
        }
    }
}
