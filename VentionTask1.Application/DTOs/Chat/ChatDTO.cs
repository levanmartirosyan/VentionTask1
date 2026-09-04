namespace VentionTask1.Application.DTOs.Chat;

public class ChatDTO
{
    public Guid Id { get; set; }
    public required ChatParticipantDTO Participant { get; set; }
    public string LastMessage { get; set; } = string.Empty;
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}
