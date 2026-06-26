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

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _dbContext.Products.ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(Guid id)
        {
            return await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            await _dbContext.Products.AddAsync(product);

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

        public async Task<bool> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}
