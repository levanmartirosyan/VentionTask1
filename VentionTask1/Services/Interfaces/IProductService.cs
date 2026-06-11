using VentionTask1.DTOs;

namespace VentionTask1.Services.Interfaces
{
    public interface IProductService
    {
        List<ProductDTO> GetAllProducts();
        ProductDTO? GetProductById(int id);
        ProductDTO CreateProduct(CreateProductDTO createProductDTO);
        ProductDTO UpdateProduct(int id, UpdateProductDTO updateProductDTO);
        bool DeleteProduct(int id);
    }
}
