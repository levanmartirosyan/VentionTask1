using VentionTask1.Application.DTOs.Chat;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Extensions;

public static class ChatMappingExtensions
{
    public static ChatDTO ToDto(this ChatSession chat, Guid currentUserId)
    {
        var participant = chat.ParticipantOneId == currentUserId
            ? chat.ParticipantTwo
            : chat.ParticipantOne;

        return new ChatDTO
        {
            Id = chat.Id,
            Participant = participant!.ToParticipantDto(),
            LastMessage = chat.LastMessage,
            LastMessageAt = chat.LastMessageAt,
            UnreadCount = 0
        };
    }

    public static ChatMessageDTO ToDto(this ChatMessage message, Guid currentUserId)
    {
        return new ChatMessageDTO
        {
            Id = message.Id,
            ChatId = message.ChatSessionId,
            Content = message.Content,
            SenderId = message.SenderId,
            SenderName = message.Sender?.Name ?? string.Empty,
            CreatedAt = message.CreatedAt,
            IsOwn = message.SenderId == currentUserId
        };
    }

    private static ChatParticipantDTO ToParticipantDto(this User user)
    {
        return new ChatParticipantDTO
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }
}
