namespace VentionTask1.DTOs
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required decimal Price { get; set; }
    }

    public class CreateProductDTO
    {
        public required string Name { get; set; }
        public required decimal Price { get; set; }
    }

    public class UpdateProductDTO
    {
        public string? Name { get; set; }
        public decimal? Price { get; set; }
    }
}
