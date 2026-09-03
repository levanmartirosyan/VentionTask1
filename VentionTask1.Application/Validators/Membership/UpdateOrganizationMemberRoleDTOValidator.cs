using FluentValidation;
using VentionTask1.Application.DTOs.Membership;

namespace VentionTask1.Application.Validators.Membership
{
    public class UpdateOrganizationMemberRoleDTOValidator : AbstractValidator<UpdateOrganizationMemberRoleDTO>
    {
        public UpdateOrganizationMemberRoleDTOValidator()
        {
            //RuleFor(x => x.Role)
            //    .IsInEnum()
            //    .Must(role => role != RoleType.OWNER)
            //    .WithMessage("Owner role cannot be assigned through this endpoint.");
        }
    }
}
