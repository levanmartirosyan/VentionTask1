using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Infrastructure.Data.Configurations
{
    public class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
    {
        public void Configure(EntityTypeBuilder<ChatSession> builder)
        {
            builder.ToTable("ChatSessions");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new
            {
                x.OrganizationId,
                x.ParticipantOneId,
                x.ParticipantTwoId
            }).IsUnique();

            builder.Property(x => x.LastMessage)
                .HasMaxLength(500);

            builder.HasOne(x => x.Organization)
                .WithMany(x => x.ChatSessions)
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ParticipantOne)
                .WithMany(x => x.ParticipantOneChatSessions)
                .HasForeignKey(x => x.ParticipantOneId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ParticipantTwo)
                .WithMany(x => x.ParticipantTwoChatSessions)
                .HasForeignKey(x => x.ParticipantTwoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
