using VentionTask1.Application.DTOs;
using VentionTask1.Domain.Entities;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;

namespace VentionTask1.Application.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<List<ProductDTO>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllProductsAsync();

            return products.Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price
            }).ToList();
        }

        public async Task<ProductDTO?> GetProductByIdAsync(Guid id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);

            if (product == null) return null;

            return new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price
            };
        }

        public async Task<ProductDTO> CreateProductAsync(CreateProductDTO createProductDTO)
        {
            var newProduct = new Product
            {
                Name = createProductDTO.Name,
                Price = createProductDTO.Price
            };

            var createdProduct = await _productRepository.CreateProductAsync(newProduct);

            if (!await _productRepository.SaveChangesAsync())
            {
                throw new InvalidOperationException("Internal server error occurred while saving changes.");
            }

            return new ProductDTO
            {
                Id = createdProduct.Id,
                Name = createdProduct.Name,
                Price = createdProduct.Price
            };
        }

        public async Task<ProductDTO> UpdateProductAsync(Guid id, UpdateProductDTO updateProductDTO)
        {
            var product = await _productRepository.GetProductByIdAsync(id);

            if (product == null) throw new KeyNotFoundException("Product not found");

            if (!string.IsNullOrEmpty(updateProductDTO.Name))
            {
                product.Name = updateProductDTO.Name;
            }

            if (updateProductDTO.Price.HasValue)
            {
                product.Price = updateProductDTO.Price.Value;
            }

            await _productRepository.UpdateProductAsync(product);

            if (!await _productRepository.SaveChangesAsync())
            {
                throw new InvalidOperationException("Internal server error occurred while saving changes.");
            }

            return new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price
            };
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);

            if (product == null) return false;

            await _productRepository.DeleteProductAsync(product);

            if (!await _productRepository.SaveChangesAsync())
            {
                throw new InvalidOperationException("Internal server error occurred while saving changes.");
            }

            return true;
        }
    }
}
