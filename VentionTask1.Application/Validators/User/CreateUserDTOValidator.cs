using FluentValidation;
using VentionTask1.Application.DTOs;

namespace VentionTask1.Application.Validators.User
{
    public class CreateUserDTOValidator : AbstractValidator<CreateUserDTO>
    {
        public CreateUserDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(255);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(255)
                .Matches(@"[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[0-9]")
                .WithMessage("Password must contain at least one number.")
                .Matches(@"[^a-zA-Z0-9]")
                .WithMessage("Password must contain at least one symbol.");
        }
    }
}
