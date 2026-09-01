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

            builder.Property(s => s.LoggedInAt)
                .IsRequired();

            builder.Property(s => s.LoggedOutAt)
                .IsRequired(false);

            builder.Property(s => s.IsActive)
                .IsRequired();

            builder.Property(s => s.IpAddress)
                .HasMaxLength(100);

            builder.Property(s => s.UserAgent)
                .HasMaxLength(500);

            builder.HasIndex(s => new { s.UserId, s.IsActive });

            builder.HasOne(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

