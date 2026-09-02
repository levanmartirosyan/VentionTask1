namespace VentionTask1.Application.Messaging
{
    public record FileProcessingRequestedEvent(
        Guid FileId,
        Guid? OrganizationId,
        string StorageKey,
        string ContentType);

    public record FileTextExtractedEvent(
        Guid FileId);

    public record FileChunkingCompletedEvent(
        Guid FileId);

    public record FileProcessingCompletedEvent(
        Guid FileId);
}
