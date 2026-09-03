using VentionTask1.Application.Services.Interfaces;

namespace VentionTask1.WebApi.Services.Implementation
{
    public class InMemoryUserPresenceTracker : IUserPresenceTracker
    {
        private readonly Dictionary<Guid, HashSet<string>> _onlineUsers = [];
        private readonly object _lock = new();

        public Task<bool> UserConnectedAsync(Guid userId, string connectionId)
        {
            lock (_lock)
            {
                var wasOffline = !_onlineUsers.ContainsKey(userId);

                if (!_onlineUsers.TryGetValue(userId, out var connections))
                {
                    connections = [];
                    _onlineUsers[userId] = connections;
                }

                connections.Add(connectionId);

                return Task.FromResult(wasOffline);
            }
        }

        public Task<bool> UserDisconnectedAsync(Guid userId, string connectionId)
        {
            lock (_lock)
            {
                if (!_onlineUsers.TryGetValue(userId, out var connections))
                {
                    return Task.FromResult(false);
                }

                connections.Remove(connectionId);

                if (connections.Count > 0)
                {
                    return Task.FromResult(false);
                }

                _onlineUsers.Remove(userId);

                return Task.FromResult(true);
            }
        }

        public Task<IReadOnlyCollection<Guid>> GetOnlineUsersAsync()
        {
            lock (_lock)
            {
                return Task.FromResult<IReadOnlyCollection<Guid>>(_onlineUsers.Keys.ToList());
            }
        }

        public Task<bool> IsOnlineAsync(Guid userId)
        {
            lock (_lock)
            {
                return Task.FromResult(_onlineUsers.ContainsKey(userId));
            }
        }
    }
}
