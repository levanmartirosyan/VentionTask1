namespace VentionTask1.Domain.Entities
{
    public class ChatSession : BaseEntity
    {
        public Guid OrganizationId { get; set; }
        public Organization? Organization { get; set; }

        public Guid ParticipantOneId { get; set; }
        public User? ParticipantOne { get; set; }

        public Guid ParticipantTwoId { get; set; }
        public User? ParticipantTwo { get; set; }

        public string LastMessage { get; set; } = string.Empty;
        public DateTime? LastMessageAt { get; set; }

        public ICollection<ChatMessage> Messages { get; set; } = [];
    }
}
