using VentionTask1.DTOs;

namespace VentionTask1.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductDTO>> GetAllProductsAsync();
        Task<ProductDTO?> GetProductByIdAsync(Guid id);
        Task<ProductDTO> CreateProductAsync(CreateProductDTO createProductDTO);
        Task<ProductDTO> UpdateProductAsync(Guid id, UpdateProductDTO updateProductDTO);
        Task<bool> DeleteProductAsync(Guid id);
    }
}
