namespace VentionTask1.Application.DTOs.Chat;

public class ChatMessageDTO
{
    public Guid Id { get; set; }
    public Guid ChatId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsOwn { get; set; }
}
