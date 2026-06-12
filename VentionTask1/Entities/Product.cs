namespace VentionTask1.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required decimal Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
