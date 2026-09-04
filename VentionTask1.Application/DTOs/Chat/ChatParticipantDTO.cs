namespace VentionTask1.Application.DTOs.Chat;

public class ChatParticipantDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
