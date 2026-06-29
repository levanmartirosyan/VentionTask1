using VentionTask1.Application.DTOs;

namespace VentionTask1.Application.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductDTO>> GetAllProductsAsync(CancellationToken ct);
        Task<ProductDTO?> GetProductByIdAsync(Guid id, CancellationToken ct);
        Task<ProductDTO> CreateProductAsync(CreateProductDTO createProductDTO, CancellationToken ct);
        Task<ProductDTO> UpdateProductAsync(Guid id, UpdateProductDTO updateProductDTO, CancellationToken ct);
        Task<bool> DeleteProductAsync(Guid id, CancellationToken ct);
    }
}
