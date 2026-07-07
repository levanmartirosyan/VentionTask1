using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Infrastructure.Data.Configurations
{
    public class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.ToTable("Sessions");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.RefreshToken)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.HasIndex(s => s.RefreshToken)
                   .IsUnique();

            builder.Property(s => s.ExpiresAt)
                   .IsRequired();

            builder.Property(s => s.IsRevoked)
                   .IsRequired();

            builder.HasIndex(s => new { s.UserId, s.IsRevoked });

            builder.HasOne(s => s.User)
                   .WithMany(u => u.Sessions)
                   .HasForeignKey(s => s.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasData(new Session
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                RefreshToken = "test-refresh-token",
                ExpiresAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                IsRevoked = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}

