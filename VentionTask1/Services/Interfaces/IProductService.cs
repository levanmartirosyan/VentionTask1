using VentionTask1.DTOs;

namespace VentionTask1.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductDTO>> GetAllProducts();
        Task<ProductDTO?> GetProductById(Guid id);
        Task<ProductDTO> CreateProduct(CreateProductDTO createProductDTO);
        Task<ProductDTO> UpdateProduct(Guid id, UpdateProductDTO updateProductDTO);
        Task<bool> DeleteProduct(Guid id);
    }
}
