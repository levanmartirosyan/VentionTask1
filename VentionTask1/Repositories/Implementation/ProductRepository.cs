using VentionTask1.Entities;
using VentionTask1.Repositories.Interfaces;

namespace VentionTask1.Repositories.Implementation
{
    public class ProductRepository : IProductRepository
    {
        private static readonly List<Product> Products =
        [
            new Product { Id = Guid.NewGuid(), Name = "Product 1", Price = 10.99m },
            new Product { Id = Guid.NewGuid(), Name = "Product 2", Price = 20.99m },
            new Product { Id = Guid.NewGuid(), Name = "Product 3", Price = 30.99m }
        ];

        public async Task<List<Product>> GetAllProductsAsync()
        {
            await Task.CompletedTask;
            return Products;
        }

        public async Task<Product?> GetProductByIdAsync(Guid id)
        {
            await Task.CompletedTask;
            return Products.FirstOrDefault(p => p.Id == id);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            await Task.CompletedTask;
            Products.Add(product);
            return product;
        }

        public async Task UpdateProductAsync(Product product)
        {
            await Task.CompletedTask;
        }

        public async Task DeleteProductAsync(Product product)
        {
            await Task.CompletedTask;
            Products.Remove(product);
        }
    }
}
