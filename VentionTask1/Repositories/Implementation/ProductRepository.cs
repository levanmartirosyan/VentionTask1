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

        public Task<List<Product>> GetAllProducts()
        {
            return Task.FromResult(Products);
        }

        public Task<Product?> GetProductById(Guid id)
        {
            return Task.FromResult(Products.FirstOrDefault(p => p.Id == id));
        }

        public Task<Product> CreateProduct(Product product)
        {
            Products.Add(product);
            return Task.FromResult(product);
        }

        public Task UpdateProduct(Product product)
        {
            return Task.CompletedTask;
        }

        public Task DeleteProduct(Product product)
        {
            Products.Remove(product);
            return Task.CompletedTask;
        }
    }
}
