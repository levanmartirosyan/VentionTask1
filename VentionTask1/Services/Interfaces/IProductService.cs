using VentionTask1.DTOs;

namespace VentionTask1.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductDTO>> GetAllProducts();
        Task<ProductDTO?> GetProductById(int id);
        Task<ProductDTO> CreateProduct(CreateProductDTO createProductDTO);
        Task<ProductDTO> UpdateProduct(int id, UpdateProductDTO updateProductDTO);
        Task<bool> DeleteProduct(int id);
    }
}
