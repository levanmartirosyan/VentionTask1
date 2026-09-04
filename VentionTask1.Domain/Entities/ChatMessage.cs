namespace VentionTask1.Domain.Entities
{
    public class ChatMessage : BaseEntity
    {
        public Guid ChatSessionId { get; set; }
        public ChatSession? ChatSession { get; set; }

        public Guid SenderId { get; set; }
        public User? Sender { get; set; }

        public required string Content { get; set; }
    }
}
