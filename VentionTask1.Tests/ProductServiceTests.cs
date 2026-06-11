using VentionTask1.DTOs;
using VentionTask1.Services.Implementation;

namespace VentionTask1.Tests
{
    public class ProductServiceTests
    {
        [Fact]
        public void Create_ShouldAddProduct()
        {
            var service = new ProductService();

            var dto = new CreateProductDTO
            {
                Name = "Phone",
                Price = 1000.20m
            };

            var result = service.CreateProduct(dto);

            Assert.NotNull(result);
            Assert.Equal("Phone", result.Name);
            Assert.Equal(1000.20m, result.Price);
        }

        [Fact]
        public void Delete_ShouldDeleteProduct()
        {
            var service = new ProductService();

            var created = service.CreateProduct(new CreateProductDTO
            {
                Name = "Phone",
                Price = 1000
            });

            var result = service.DeleteProduct(created.Id);

            Assert.True(result);
        }

        [Fact]
        public void Update_ShouldUpdateNameAndPrice()
        {
            var service = new ProductService();

            var created = service.CreateProduct(new CreateProductDTO
            {
                Name = "Phone",
                Price = 1000
            });

            var updateDto = new UpdateProductDTO
            {
                Name = "Laptop",
                Price = 2000
            };

            var result = service.UpdateProduct(created.Id, updateDto);

            Assert.NotNull(result);
            Assert.Equal(created.Id, result.Id);
            Assert.Equal("Laptop", result.Name);
            Assert.Equal(2000, result.Price);
        }

        [Fact]
        public void Get_ShouldGetProductById()
        {
            var service = new ProductService();

            var created = service.CreateProduct(new CreateProductDTO
            {
                Name = "Phone",
                Price = 1000
            });

            var result = service.GetProductById(created.Id);

            Assert.NotNull(result);
            Assert.Equal(created.Id, result.Id);
            Assert.Equal("Phone", result.Name);
            Assert.Equal(1000, result.Price);
        }
}
}
