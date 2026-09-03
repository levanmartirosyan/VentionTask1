using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using VentionTask1.Application.Services.Interfaces;

namespace VentionTask1.WebApi.Hubs
{
    public class PresenceHub : Hub
    {
        private readonly IUserPresenceTracker _presenceTracker;

        public PresenceHub(IUserPresenceTracker presenceTracker)
        {
            _presenceTracker = presenceTracker;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();

            if (userId.HasValue)
            {
                var becameOnline = await _presenceTracker.UserConnectedAsync(
                    userId.Value,
                    Context.ConnectionId);

                if (becameOnline)
                {
                    await Clients.All.SendAsync("UserOnline", userId.Value);
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();

            if (userId.HasValue)
            {
                var becameOffline = await _presenceTracker.UserDisconnectedAsync(
                    userId.Value,
                    Context.ConnectionId);

                if (becameOffline)
                {
                    await Clients.All.SendAsync("UserOffline", userId.Value);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public Task<IReadOnlyCollection<Guid>> GetOnlineUsers()
        {
            return _presenceTracker.GetOnlineUsersAsync();
        }

        private Guid? GetUserId()
        {
            var userIdValue = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : null;
        }
    }
}
