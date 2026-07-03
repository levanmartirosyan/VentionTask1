namespace VentionTask1.Application.DTOs
{
    public class PaginatedResponseDTO<T>
    {
        public required List<T> Items { get; set; }
        public Guid? NextCursor { get; set; }
        public bool HasNextPage { get; set; }
    }
}
