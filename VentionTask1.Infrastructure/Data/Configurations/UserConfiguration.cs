using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Username)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.HasIndex(u => u.Email)
                   .IsUnique();

            builder.Property(u => u.PasswordHash)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.HasOne(u => u.Organization)
                   .WithMany(o => o.Users)
                   .HasForeignKey(u => u.OrganizationId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasData(new User
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Username = "Admin",
                Email = "admin@example.com",
                PasswordHash = "Admin123!",
                OrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}

