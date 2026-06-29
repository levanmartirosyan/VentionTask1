using Moq;
using VentionTask1.Domain.Entities;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.DTOs;
using VentionTask1.Application.Services.Implementation;

namespace VentionTask1.Tests
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly ProductService _productService;
        private readonly CancellationToken _ct = CancellationToken.None;

        public ProductServiceTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _productService = new ProductService(_productRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllProducts_ShouldReturnProductDtos()
        {
            var products = new List<Product>
            {
                new() { Id = Guid.NewGuid(), Name = "Phone", Price = 1000 },
                new() { Id = Guid.NewGuid(), Name = "Laptop", Price = 2000 }
            };

            _productRepositoryMock
                .Setup(repository => repository.GetAllProductsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);

            var result = await _productService.GetAllProductsAsync(_ct);

            Assert.Equal(2, result.Count);
            Assert.Equal("Phone", result[0].Name);
            Assert.Equal(1000, result[0].Price);
            Assert.Equal("Laptop", result[1].Name);
            Assert.Equal(2000, result[1].Price);
            _productRepositoryMock.Verify(repository => repository.GetAllProductsAsync(_ct), Times.Once);
        }

        [Fact]
        public async Task GetAllProducts_WhenRepositoryIsEmpty_ShouldReturnEmptyList()
        {
            _productRepositoryMock
                .Setup(repository => repository.GetAllProductsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var result = await _productService.GetAllProductsAsync(_ct);

            Assert.Empty(result);
            _productRepositoryMock.Verify(repository => repository.GetAllProductsAsync(_ct), Times.Once);
        }

        [Fact]
        public async Task GetProductById_WhenProductExists_ShouldReturnProductDto()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Phone",
                Price = 1000
            };

            _productRepositoryMock
                .Setup(repository => repository.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var result = await _productService.GetProductByIdAsync(productId, _ct);

            Assert.NotNull(result);
            Assert.Equal(productId, result.Id);
            Assert.Equal("Phone", result.Name);
            Assert.Equal(1000, result.Price);
            _productRepositoryMock.Verify(repository => repository.GetProductByIdAsync(productId, _ct), Times.Once);
        }

        [Fact]
        public async Task GetProductById_WhenProductDoesNotExist_ShouldReturnNull()
        {
            var productId = Guid.NewGuid();

            _productRepositoryMock
                .Setup(repository => repository.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            var result = await _productService.GetProductByIdAsync(productId, _ct);

            Assert.Null(result);
            _productRepositoryMock.Verify(repository => repository.GetProductByIdAsync(productId, _ct), Times.Once);
        }

        [Fact]
        public async Task CreateProduct_ShouldCreateProductWithNewId()
        {
            var createDto = new CreateProductDTO
            {
                Name = "Tablet",
                Price = 1500
            };

            _productRepositoryMock
                .Setup(repository => repository.CreateProductAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product product, CancellationToken _) =>
                {
                    product.Id = Guid.NewGuid();
                    return product;
                });

            _productRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _productService.CreateProductAsync(createDto, _ct);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("Tablet", result.Name);
            Assert.Equal(1500, result.Price);
            _productRepositoryMock.Verify(repository => repository.CreateProductAsync(It.Is<Product>(product =>
                product.Name == "Tablet" &&
                product.Price == 1500), _ct), Times.Once);
            _productRepositoryMock.Verify(repository => repository.SaveChangesAsync(_ct), Times.Once);
        }

        [Fact]
        public async Task CreateProduct_WhenRepositoryFails_ShouldThrowInvalidOperationException()
        {
            var createDto = new CreateProductDTO
            {
                Name = "Phone",
                Price = 1000
            };

            _productRepositoryMock
                .Setup(repository => repository.CreateProductAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Create failed"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _productService.CreateProductAsync(createDto, _ct));
            _productRepositoryMock.Verify(repository => repository.CreateProductAsync(It.Is<Product>(product =>
                product.Name == "Phone" &&
                product.Price == 1000), _ct), Times.Once);
            _productRepositoryMock.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProduct_WhenProductExists_ShouldUpdateNameAndPrice()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Phone",
                Price = 1000
            };

            var updateDto = new UpdateProductDTO
            {
                Name = "Laptop",
                Price = 2000
            };

            _productRepositoryMock
                .Setup(repository => repository.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            _productRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _productService.UpdateProductAsync(productId, updateDto, _ct);

            Assert.Equal(productId, result.Id);
            Assert.Equal("Laptop", result.Name);
            Assert.Equal(2000, result.Price);
            _productRepositoryMock.Verify(repository => repository.UpdateProductAsync(It.Is<Product>(updatedProduct =>
                updatedProduct.Id == productId &&
                updatedProduct.Name == "Laptop" &&
                updatedProduct.Price == 2000)), Times.Once);
            _productRepositoryMock.Verify(repository => repository.SaveChangesAsync(_ct), Times.Once);
        }

        [Fact]
        public async Task UpdateProduct_WhenProductDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            var productId = Guid.NewGuid();
            var updateDto = new UpdateProductDTO
            {
                Name = "Laptop",
                Price = 2000
            };

            _productRepositoryMock
                .Setup(repository => repository.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _productService.UpdateProductAsync(productId, updateDto, _ct));
            _productRepositoryMock.Verify(repository => repository.UpdateProductAsync(It.IsAny<Product>()), Times.Never);
            _productRepositoryMock.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteProduct_WhenProductExists_ShouldDeleteProduct()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Phone",
                Price = 1000
            };

            _productRepositoryMock
                .Setup(repository => repository.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            _productRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _productService.DeleteProductAsync(productId, _ct);

            Assert.True(result);
            _productRepositoryMock.Verify(repository => repository.DeleteProductAsync(product), Times.Once);
            _productRepositoryMock.Verify(repository => repository.SaveChangesAsync(_ct), Times.Once);
        }

        [Fact]
        public async Task DeleteProduct_WhenProductDoesNotExist_ShouldReturnFalse()
        {
            var productId = Guid.NewGuid();

            _productRepositoryMock
                .Setup(repository => repository.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            var result = await _productService.DeleteProductAsync(productId, _ct);

            Assert.False(result);
            _productRepositoryMock.Verify(repository => repository.DeleteProductAsync(It.IsAny<Product>()), Times.Never);
            _productRepositoryMock.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
