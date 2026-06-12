using VentionTask1.DTOs;
using VentionTask1.Entities;
using VentionTask1.Repositories.Interfaces;
using VentionTask1.Services.Interfaces;

namespace VentionTask1.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<List<ProductDTO>> GetAllProducts()
        {
            var products = await _productRepository.GetAllProducts();

            return products.Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price
            }).ToList();
        }

        public async Task<ProductDTO?> GetProductById(Guid id)
        {
            var product = await _productRepository.GetProductById(id);

            if (product == null) return null;

            return new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price
            };
        }

        public async Task<ProductDTO> CreateProduct(CreateProductDTO createProductDTO)
        {
            var newProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = createProductDTO.Name,
                Price = createProductDTO.Price
            };

            var createdProduct = await _productRepository.CreateProduct(newProduct);

            return new ProductDTO
            {
                Id = createdProduct.Id,
                Name = createdProduct.Name,
                Price = createdProduct.Price
            };
        }

        public async Task<ProductDTO> UpdateProduct(Guid id, UpdateProductDTO updateProductDTO)
        {
            var product = await _productRepository.GetProductById(id);

            if (product == null) throw new KeyNotFoundException("Product not found");

            if (!string.IsNullOrEmpty(updateProductDTO.Name))
            {
                product.Name = updateProductDTO.Name;
            }

            if (updateProductDTO.Price.HasValue)
            {
                product.Price = updateProductDTO.Price.Value;
            }

            await _productRepository.UpdateProduct(product);

            return new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price
            };
        }

        public async Task<bool> DeleteProduct(Guid id)
        {
            var product = await _productRepository.GetProductById(id);

            if (product == null) return false;

            await _productRepository.DeleteProduct(product);

            return true;
        }
    }
}
