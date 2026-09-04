using FluentValidation;
using VentionTask1.Application.DTOs.Chat;

namespace VentionTask1.Application.Validators.Chat;

public class CreateChatSessionDTOValidator : AbstractValidator<CreateChatSessionDTO>
{
    public CreateChatSessionDTOValidator()
    {
        RuleFor(x => x.ReceiverId)
            .NotEmpty();
    }
}
