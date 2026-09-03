namespace VentionTask1.Application.Services.Interfaces
{
    public interface IUserPresenceTracker
    {
        Task<bool> UserConnectedAsync(Guid userId, string connectionId);
        Task<bool> UserDisconnectedAsync(Guid userId, string connectionId);
        Task<IReadOnlyCollection<Guid>> GetOnlineUsersAsync();
        Task<bool> IsOnlineAsync(Guid userId);
    }
}
