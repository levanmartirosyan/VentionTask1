using Microsoft.EntityFrameworkCore;
using VentionTask1.Entities;

namespace VentionTask1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
    }
}
