using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Infrastructure.Data.Configurations
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.ToTable("ChatMessages");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new
            {
                x.ChatSessionId,
                x.CreatedAt
            });

            builder.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(2000);

            builder.HasOne(x => x.ChatSession)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.ChatSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Sender)
                .WithMany(x => x.ChatMessages)
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
