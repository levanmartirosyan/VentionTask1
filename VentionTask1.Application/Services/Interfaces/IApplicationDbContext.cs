using Microsoft.EntityFrameworkCore;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Services.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; set; }
        DbSet<Organization> Organizations { get; set; }
        DbSet<Session> Sessions { get; set; }
        DbSet<UploadedFile> Files { get; set; }
        DbSet<FileChunk> FileChunks { get; set; }
        DbSet<OrganizationMember> OrganizationMembers { get; set; }

        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
