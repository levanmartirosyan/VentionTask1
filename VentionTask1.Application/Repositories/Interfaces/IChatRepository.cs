using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Repositories.Interfaces;

public interface IChatRepository
{
    Task<List<ChatSession>> GetUserChatsAsync(Guid userId, Guid organizationId, Guid? cursor, int pageSize, CancellationToken ct);
    Task<ChatSession?> GetByUsersAsync(Guid organizationId, Guid participantOneId, Guid participantTwoId, CancellationToken ct);
    Task<ChatSession?> GetByIdAsync(Guid chatId, CancellationToken ct);
    Task<List<ChatMessage>> GetMessagesAsync(Guid chatId, Guid? cursor, int pageSize, CancellationToken ct);
    Task AddChatAsync(ChatSession chat, CancellationToken ct);
    Task AddMessageAsync(ChatMessage message, CancellationToken ct);
    Task<bool> SaveChangesAsync(CancellationToken ct);
}
