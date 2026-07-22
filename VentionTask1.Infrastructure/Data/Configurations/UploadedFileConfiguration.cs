using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Infrastructure.Data.Configurations
{
    public class UploadedFileConfiguration : IEntityTypeConfiguration<UploadedFile>
    {
        public void Configure(EntityTypeBuilder<UploadedFile> builder)
        {
            builder.ToTable("Files");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Filename)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(f => f.StoredFileName)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(f => f.Status)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(f => f.ContentType)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(f => f.Checksum)
                   .IsRequired()
                   .HasMaxLength(128);

            builder.Property(f => f.StorageKey)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(f => f.Application)
                   .HasMaxLength(100);

            builder.Property(f => f.ProcessingError)
                   .HasMaxLength(1000);

            builder.HasOne(f => f.Organization)
                   .WithMany(o => o.Files)
                   .HasForeignKey(f => f.OrganizationId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(f => f.Owner)
                   .WithMany(u => u.Files)
                   .HasForeignKey(f => f.OwnerId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
