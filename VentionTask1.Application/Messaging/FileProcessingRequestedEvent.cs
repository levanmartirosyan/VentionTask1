namespace VentionTask1.Application.Messaging
{
    public record FileProcessingRequestedEvent(
        Guid FileId,
        Guid? OrganizationId,
        string StorageKey,
        string ContentType);

    public record FileTextExtractionRequestedEvent(
        Guid FileId);

    public record FileChunkingRequestedEvent(
        Guid FileId);

    public record FileProcessingCompletionRequestedEvent(
        Guid FileId);
}
