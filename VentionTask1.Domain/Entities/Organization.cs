namespace VentionTask1.Domain.Entities
{
    public class Organization : BaseEntity
    {
        public required string Name { get; set; }
        public ICollection<User> Users { get; set; } = [];
        public ICollection<UploadedFile> Files { get; set; } = [];
    }
}
