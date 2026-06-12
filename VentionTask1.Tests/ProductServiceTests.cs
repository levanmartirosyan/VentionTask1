using Moq;
using VentionTask1.DTOs;
using VentionTask1.Entities;
using VentionTask1.Repositories.Interfaces;
using VentionTask1.Services.Implementation;

namespace VentionTask1.Tests
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly ProductService _productService;

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
                .Setup(repository => repository.GetAllProducts())
                .ReturnsAsync(products);

            var result = await _productService.GetAllProducts();

            Assert.Equal(2, result.Count);
            Assert.Equal("Phone", result[0].Name);
            Assert.Equal(1000, result[0].Price);
            Assert.Equal("Laptop", result[1].Name);
            Assert.Equal(2000, result[1].Price);
            _productRepositoryMock.Verify(repository => repository.GetAllProducts(), Times.Once);
        }

        [Fact]
        public async Task GetAllProducts_WhenRepositoryIsEmpty_ShouldReturnEmptyList()
        {
            _productRepositoryMock
                .Setup(repository => repository.GetAllProducts())
                .ReturnsAsync([]);

            var result = await _productService.GetAllProducts();

            Assert.Empty(result);
            _productRepositoryMock.Verify(repository => repository.GetAllProducts(), Times.Once);
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
                .Setup(repository => repository.GetProductById(productId))
                .ReturnsAsync(product);

            var result = await _productService.GetProductById(productId);

            Assert.NotNull(result);
            Assert.Equal(productId, result.Id);
            Assert.Equal("Phone", result.Name);
            Assert.Equal(1000, result.Price);
            _productRepositoryMock.Verify(repository => repository.GetProductById(productId), Times.Once);
        }

        [Fact]
        public async Task GetProductById_WhenProductDoesNotExist_ShouldReturnNull()
        {
            var productId = Guid.NewGuid();

            _productRepositoryMock
                .Setup(repository => repository.GetProductById(productId))
                .ReturnsAsync((Product?)null);

            var result = await _productService.GetProductById(productId);

            Assert.Null(result);
            _productRepositoryMock.Verify(repository => repository.GetProductById(productId), Times.Once);
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
                .Setup(repository => repository.CreateProduct(It.IsAny<Product>()))
                .ReturnsAsync((Product product) => product);

            var result = await _productService.CreateProduct(createDto);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("Tablet", result.Name);
            Assert.Equal(1500, result.Price);
            _productRepositoryMock.Verify(repository => repository.CreateProduct(It.Is<Product>(product =>
                product.Id != Guid.Empty &&
                product.Name == "Tablet" &&
                product.Price == 1500)), Times.Once);
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
                .Setup(repository => repository.CreateProduct(It.IsAny<Product>()))
                .ThrowsAsync(new InvalidOperationException("Create failed"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _productService.CreateProduct(createDto));
            _productRepositoryMock.Verify(repository => repository.CreateProduct(It.Is<Product>(product =>
                product.Id != Guid.Empty &&
                product.Name == "Phone" &&
                product.Price == 1000)), Times.Once);
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
                .Setup(repository => repository.GetProductById(productId))
                .ReturnsAsync(product);

            var result = await _productService.UpdateProduct(productId, updateDto);

            Assert.Equal(productId, result.Id);
            Assert.Equal("Laptop", result.Name);
            Assert.Equal(2000, result.Price);
            _productRepositoryMock.Verify(repository => repository.UpdateProduct(It.Is<Product>(updatedProduct =>
                updatedProduct.Id == productId &&
                updatedProduct.Name == "Laptop" &&
                updatedProduct.Price == 2000)), Times.Once);
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
                .Setup(repository => repository.GetProductById(productId))
                .ReturnsAsync((Product?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _productService.UpdateProduct(productId, updateDto));
            _productRepositoryMock.Verify(repository => repository.UpdateProduct(It.IsAny<Product>()), Times.Never);
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
                .Setup(repository => repository.GetProductById(productId))
                .ReturnsAsync(product);

            var result = await _productService.DeleteProduct(productId);

            Assert.True(result);
            _productRepositoryMock.Verify(repository => repository.DeleteProduct(product), Times.Once);
        }

        [Fact]
        public async Task DeleteProduct_WhenProductDoesNotExist_ShouldReturnFalse()
        {
            var productId = Guid.NewGuid();

            _productRepositoryMock
                .Setup(repository => repository.GetProductById(productId))
                .ReturnsAsync((Product?)null);

            var result = await _productService.DeleteProduct(productId);

            Assert.False(result);
            _productRepositoryMock.Verify(repository => repository.DeleteProduct(It.IsAny<Product>()), Times.Never);
        }
    }
}
