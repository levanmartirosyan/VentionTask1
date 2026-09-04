using Microsoft.EntityFrameworkCore;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Infrastructure.Repositories.Implementation;

public class ChatRepository : IChatRepository
{
    private readonly IApplicationDbContext _dbContext;

    public ChatRepository(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ChatSession>> GetUserChatsAsync(Guid userId, Guid organizationId, Guid? cursor, int pageSize, CancellationToken ct)
    {
        var query = _dbContext.ChatSessions
            .AsNoTracking()
            .Include(chat => chat.ParticipantOne)
            .Include(chat => chat.ParticipantTwo)
            .Where(chat =>
                chat.OrganizationId == organizationId &&
                (chat.ParticipantOneId == userId || chat.ParticipantTwoId == userId))
            .OrderByDescending(chat => chat.LastMessageAt ?? chat.CreatedAt)
            .AsQueryable();

        if (cursor.HasValue)
        {
            query = query.Where(chat => chat.Id != cursor.Value);
        }

        return await query
            .Take(pageSize + 1)
            .ToListAsync(ct);
    }

    public async Task<ChatSession?> GetByUsersAsync(Guid organizationId, Guid participantOneId, Guid participantTwoId, CancellationToken ct)
    {
        return await _dbContext.ChatSessions
            .Include(chat => chat.ParticipantOne)
            .Include(chat => chat.ParticipantTwo)
            .FirstOrDefaultAsync(chat =>
                chat.OrganizationId == organizationId &&
                chat.ParticipantOneId == participantOneId &&
                chat.ParticipantTwoId == participantTwoId,
                ct);
    }

    public async Task<ChatSession?> GetByIdAsync(Guid chatId, CancellationToken ct)
    {
        return await _dbContext.ChatSessions
            .Include(chat => chat.ParticipantOne)
            .Include(chat => chat.ParticipantTwo)
            .FirstOrDefaultAsync(chat => chat.Id == chatId, ct);
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(Guid chatId, Guid? cursor, int pageSize, CancellationToken ct)
    {
        var query = _dbContext.ChatMessages
            .AsNoTracking()
            .Include(message => message.Sender)
            .Where(message => message.ChatSessionId == chatId)
            .OrderByDescending(message => message.CreatedAt)
            .AsQueryable();

        if (cursor.HasValue)
        {
            query = query.Where(message => message.Id != cursor.Value);
        }

        return await query
            .Take(pageSize + 1)
            .ToListAsync(ct);
    }

    public async Task AddChatAsync(ChatSession chat, CancellationToken ct)
    {
        await _dbContext.ChatSessions.AddAsync(chat, ct);
    }

    public async Task AddMessageAsync(ChatMessage message, CancellationToken ct)
    {
        await _dbContext.ChatMessages.AddAsync(message, ct);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken ct)
    {
        return await _dbContext.SaveChangesAsync(ct) > 0;
    }
}
