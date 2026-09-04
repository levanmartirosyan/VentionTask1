using FluentValidation;
using VentionTask1.Application.DTOs.Chat;

namespace VentionTask1.Application.Validators.Chat;

public class SendChatMessageDTOValidator : AbstractValidator<SendChatMessageDTO>
{
    public SendChatMessageDTOValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
