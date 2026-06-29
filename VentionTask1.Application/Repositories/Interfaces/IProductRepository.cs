using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllProductsAsync(CancellationToken ct);
        Task<Product?> GetProductByIdAsync(Guid id, CancellationToken ct);
        Task<Product> CreateProductAsync(Product product, CancellationToken ct);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(Product product);
        Task<bool> SaveChangesAsync(CancellationToken ct);
    }
}
