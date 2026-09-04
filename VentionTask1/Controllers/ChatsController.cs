using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VentionTask1.Application.DTOs.Chat;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.WebApi.Extensions;

namespace VentionTask1.WebApi.Controllers;

[Authorize]
[Route("api/chats")]
[ApiController]
public class ChatsController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatsController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetChatsAsync(
        [FromHeader(Name = "x-org-id")] Guid organizationId,
        [FromQuery] Guid? cursor,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = User.GetUserId();

        var result = await _chatService.GetChatsAsync(userId, organizationId, cursor, pageSize, ct);

        return Ok(result);
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> CreateChatAsync(
        [FromHeader(Name = "x-org-id")] Guid organizationId,
        [FromBody] CreateChatSessionDTO dto,
        CancellationToken ct)
    {
        var userId = User.GetUserId();

        var result = await _chatService.CreateChatAsync(userId, organizationId, dto, ct);

        return Ok(result);
    }

    [HttpGet("sessions/{chatId:guid}/messages")]
    public async Task<IActionResult> GetMessagesAsync(
        Guid chatId,
        [FromQuery] Guid? cursor,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = User.GetUserId();

        var result = await _chatService.GetMessagesAsync(userId, chatId, cursor, pageSize, ct);

        return Ok(result);
    }

    [HttpPost("sessions/{chatId:guid}/messages")]
    public async Task<IActionResult> SendMessageAsync(
        Guid chatId,
        [FromBody] SendChatMessageDTO dto,
        CancellationToken ct)
    {
        var userId = User.GetUserId();

        var result = await _chatService.SendMessageAsync(userId, chatId, dto, ct);

        return Ok(result);
    }
}
