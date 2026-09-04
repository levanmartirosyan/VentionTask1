using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<UploadedFile> Files { get; set; }
        public DbSet<FileChunk> FileChunks { get; set; }
        public DbSet<OrganizationMember> OrganizationMembers { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            ApplyAuditValues();
            return base.SaveChangesAsync(ct);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        private void ApplyAuditValues()
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    continue;
                }

                if (entry.State != EntityState.Modified)
                {
                    continue;
                }

                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
