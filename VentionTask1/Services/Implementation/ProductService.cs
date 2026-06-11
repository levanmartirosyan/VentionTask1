using VentionTask1.DTOs;
using VentionTask1.Entities;
using VentionTask1.Services.Interfaces;

namespace VentionTask1.Services.Implementation
{
    public class ProductService : IProductService
    {
        private static readonly List<Product> Products =
            [ 
              new Product { Id = 1, Name = "Product 1", Price = 10.99m },
              new Product { Id = 2, Name = "Product 2", Price = 20.99m },
              new Product { Id = 3, Name = "Product 3", Price = 30.99m }
            ];

        public List<ProductDTO> GetAllProducts()
        {
            return Products.Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price
            }).ToList();
        }

        public ProductDTO? GetProductById(int id)
        {
            var product =  Products.FirstOrDefault(p => p.Id == id);

            if (product == null) return null;

            return new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price
            };
        }

        public ProductDTO CreateProduct(CreateProductDTO createProductDTO)
        {
            var newProduct = new Product
            {
                Id = Products.Count > 0 ? Products.Max(p => p.Id) + 1 : 1,
                Name = createProductDTO.Name,
                Price = createProductDTO.Price
            };

            Products.Add(newProduct);

            return new ProductDTO
            {
                Id = newProduct.Id,
                Name = newProduct.Name,
                Price = newProduct.Price
            };
        }

        public ProductDTO UpdateProduct(int id, UpdateProductDTO updateProductDTO)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);

            if (product == null) throw new KeyNotFoundException("Product not found");

            if (!string.IsNullOrEmpty(updateProductDTO.Name))
            {
                product.Name = updateProductDTO.Name;
            }

            if (updateProductDTO.Price.HasValue)
            {
                product.Price = updateProductDTO.Price.Value;
            }

            return new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price
            };
        }

        public bool DeleteProduct(int id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);

            if (product == null) return false;

            Products.Remove(product);

            return true;
        }
    }
}
