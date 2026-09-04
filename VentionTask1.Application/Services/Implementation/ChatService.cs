using FluentValidation;
using VentionTask1.Application.DTOs;
using VentionTask1.Application.DTOs.Chat;
using VentionTask1.Application.Exceptions;
using VentionTask1.Application.Extensions;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Constants;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Services.Implementation;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationMemberRepository _memberRepository;
    private readonly IOrganizationPermissionService _permissionService;
    private readonly IValidator<CreateChatSessionDTO> _createValidator;
    private readonly IValidator<SendChatMessageDTO> _messageValidator;

    public ChatService(
        IChatRepository chatRepository,
        IUserRepository userRepository,
        IOrganizationMemberRepository memberRepository,
        IOrganizationPermissionService permissionService,
        IValidator<CreateChatSessionDTO> createValidator,
        IValidator<SendChatMessageDTO> messageValidator)
    {
        _chatRepository = chatRepository;
        _userRepository = userRepository;
        _memberRepository = memberRepository;
        _permissionService = permissionService;
        _createValidator = createValidator;
        _messageValidator = messageValidator;
    }

    public async Task<PaginatedResponseDTO<ChatDTO>> GetChatsAsync(Guid userId, Guid organizationId, Guid? cursor, int pageSize, CancellationToken ct)
    {
        pageSize = NormalizePageSize(pageSize);

        await _permissionService.EnsureCanAccessOrganizationAsync(userId, RoleType.MEMBER, organizationId, ct);

        var chats = await _chatRepository.GetUserChatsAsync(userId, organizationId, cursor, pageSize, ct);
        var hasNextPage = chats.Count > pageSize;

        var items = chats
            .Take(pageSize)
            .Select(chat => chat.ToDto(userId))
            .ToList();

        return new PaginatedResponseDTO<ChatDTO>
        {
            Items = items,
            HasNextPage = hasNextPage,
            NextCursor = hasNextPage && items.Any()
                ? items.Last().Id
                : null
        };
    }

    public async Task<ChatDTO> CreateChatAsync(Guid userId, Guid organizationId, CreateChatSessionDTO dto, CancellationToken ct)
    {
        var validationResult = await _createValidator.ValidateAsync(dto, ct);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        if (userId == dto.ReceiverId)
        {
            throw new InvalidOperationException("Cannot create chat with yourself.");
        }

        await EnsureOrganizationMemberAsync(organizationId, userId, ct);
        await EnsureOrganizationMemberAsync(organizationId, dto.ReceiverId, ct);

        var receiver = await _userRepository.GetUserByIdAsync(dto.ReceiverId, ct);

        if (receiver == null)
        {
            throw new KeyNotFoundException($"User with ID '{dto.ReceiverId}' was not found.");
        }

        var participantIds = SortParticipants(userId, dto.ReceiverId);

        var existingChat = await _chatRepository.GetByUsersAsync(
            organizationId,
            participantIds.ParticipantOneId,
            participantIds.ParticipantTwoId,
            ct);

        if (existingChat != null)
        {
            return existingChat.ToDto(userId);
        }

        var chat = new ChatSession
        {
            OrganizationId = organizationId,
            ParticipantOneId = participantIds.ParticipantOneId,
            ParticipantTwoId = participantIds.ParticipantTwoId
        };

        await _chatRepository.AddChatAsync(chat, ct);

        if (!await _chatRepository.SaveChangesAsync(ct))
        {
            throw new InvalidOperationException("Chat could not be created.");
        }

        var createdChat = await _chatRepository.GetByIdAsync(chat.Id, ct);

        return createdChat!.ToDto(userId);
    }

    public async Task<PaginatedResponseDTO<ChatMessageDTO>> GetMessagesAsync(Guid userId, Guid chatId, Guid? cursor, int pageSize, CancellationToken ct)
    {
        pageSize = NormalizePageSize(pageSize);

        var chat = await EnsureChatParticipantAsync(chatId, userId, ct);
        await EnsureOrganizationMemberAsync(chat.OrganizationId, userId, ct);

        var messages = await _chatRepository.GetMessagesAsync(chatId, cursor, pageSize, ct);
        var hasNextPage = messages.Count > pageSize;

        var items = messages
            .Take(pageSize)
            .Select(message => message.ToDto(userId))
            .Reverse()
            .ToList();

        return new PaginatedResponseDTO<ChatMessageDTO>
        {
            Items = items,
            HasNextPage = hasNextPage,
            NextCursor = hasNextPage && messages.Any()
                ? messages.Take(pageSize).Last().Id
                : null
        };
    }

    public async Task<ChatMessageDTO> SendMessageAsync(Guid userId, Guid chatId, SendChatMessageDTO dto, CancellationToken ct)
    {
        var validationResult = await _messageValidator.ValidateAsync(dto, ct);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var chat = await EnsureChatParticipantAsync(chatId, userId, ct);
        await EnsureOrganizationMemberAsync(chat.OrganizationId, userId, ct);

        var content = dto.Content.Trim();

        var message = new ChatMessage
        {
            ChatSessionId = chatId,
            SenderId = userId,
            Content = content
        };

        chat.LastMessage = content.Length > 500
            ? content[..500]
            : content;
        chat.LastMessageAt = DateTime.UtcNow;

        await _chatRepository.AddMessageAsync(message, ct);

        if (!await _chatRepository.SaveChangesAsync(ct))
        {
            throw new InvalidOperationException("Chat message could not be sent.");
        }

        message.Sender = userId == chat.ParticipantOneId
            ? chat.ParticipantOne
            : chat.ParticipantTwo;

        return message.ToDto(userId);
    }

    private async Task EnsureOrganizationMemberAsync(Guid organizationId, Guid userId, CancellationToken ct)
    {
        var member = await _memberRepository.GetByOrganizationAndUserAsync(organizationId, userId, ct);

        if (member == null)
        {
            throw new ForbiddenAccessException("User does not have access to this organization.");
        }
    }

    private async Task<ChatSession> EnsureChatParticipantAsync(Guid chatId, Guid userId, CancellationToken ct)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId, ct);

        if (chat == null)
        {
            throw new KeyNotFoundException($"Chat with ID '{chatId}' was not found.");
        }

        if (chat.ParticipantOneId != userId && chat.ParticipantTwoId != userId)
        {
            throw new ForbiddenAccessException("You do not have access to this chat.");
        }

        return chat;
    }

    private static int NormalizePageSize(int pageSize)
    {
        if (pageSize <= 0)
        {
            return 10;
        }

        return pageSize > 100
            ? 100
            : pageSize;
    }

    private static (Guid ParticipantOneId, Guid ParticipantTwoId) SortParticipants(Guid firstUserId, Guid secondUserId)
    {
        return firstUserId.CompareTo(secondUserId) < 0
            ? (firstUserId, secondUserId)
            : (secondUserId, firstUserId);
    }
}
