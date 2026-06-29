using Microsoft.EntityFrameworkCore;
using VentionTask1.Infrastructure.Data;
using VentionTask1.Domain.Entities;
using VentionTask1.Application.Repositories.Interfaces;

namespace VentionTask1.Infrastructure.Repositories.Implementation
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Product>> GetAllProductsAsync(CancellationToken ct)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<Product?> GetProductByIdAsync(Guid id, CancellationToken ct)
        {
            return await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task<Product> CreateProductAsync(Product product, CancellationToken ct)
        {
            await _dbContext.Products.AddAsync(product, ct);

            return product;
        }

        public Task UpdateProductAsync(Product product)
        {
            _dbContext.Products.Update(product);

            return Task.CompletedTask;
        }

        public Task DeleteProductAsync(Product product)
        {
            _dbContext.Products.Remove(product);

            return Task.CompletedTask;
        }

        public async Task<bool> SaveChangesAsync(CancellationToken ct)
        {
            return await _dbContext.SaveChangesAsync(ct) > 0;
        }
    }
}
