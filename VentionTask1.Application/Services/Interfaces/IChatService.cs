using VentionTask1.Application.DTOs;
using VentionTask1.Application.DTOs.Chat;

namespace VentionTask1.Application.Services.Interfaces;

public interface IChatService
{
    Task<PaginatedResponseDTO<ChatDTO>> GetChatsAsync(Guid userId, Guid organizationId, Guid? cursor, int pageSize, CancellationToken ct);
    Task<ChatDTO> CreateChatAsync(Guid userId, Guid organizationId, CreateChatSessionDTO dto, CancellationToken ct);
    Task<PaginatedResponseDTO<ChatMessageDTO>> GetMessagesAsync(Guid userId, Guid chatId, Guid? cursor, int pageSize, CancellationToken ct);
    Task<ChatMessageDTO> SendMessageAsync(Guid userId, Guid chatId, SendChatMessageDTO dto, CancellationToken ct);
}
