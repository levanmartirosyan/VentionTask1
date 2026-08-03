using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Infrastructure.Data.Configurations
{
    public class FileChunkConfiguration : IEntityTypeConfiguration<FileChunk>
    {
        public void Configure(EntityTypeBuilder<FileChunk> builder)
        {
            builder.ToTable("FileChunks");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.ChunkIndex)
                .IsRequired();

            builder.HasOne(x => x.File)
                .WithMany()
                .HasForeignKey(x => x.FileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
