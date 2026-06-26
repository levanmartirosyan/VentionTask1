using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(Guid id);
        Task<Product> CreateProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(Product product);
        Task<bool> SaveChangesAsync();
    }
}
