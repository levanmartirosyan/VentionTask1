namespace VentionTask1.Application.DTOs
{
    public class ProductDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required decimal Price { get; set; }
    }
}
