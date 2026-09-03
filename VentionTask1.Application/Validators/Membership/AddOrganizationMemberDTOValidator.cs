using FluentValidation;
using VentionTask1.Application.DTOs.Membership;
using VentionTask1.Domain.Constants;

namespace VentionTask1.Application.Validators.Membership
{
    public class AddOrganizationMemberDTOValidator : AbstractValidator<AddOrganizationMemberDTO>
    {
        public AddOrganizationMemberDTOValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(255);

            //RuleFor(x => x.Role)
            //    .IsInEnum()
            //    .Must(role => role != RoleType.OWNER)
            //    .WithMessage("Owner role cannot be assigned through this endpoint.");
        }
    }
}
