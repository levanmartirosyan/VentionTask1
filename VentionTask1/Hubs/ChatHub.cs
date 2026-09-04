using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace VentionTask1.WebApi.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendPrivateMessage(Guid receiverId, string content)
        {
            var senderIdValue = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(senderIdValue, out var senderId))
            {
                throw new HubException("Unauthorized.");
            }

            var text = content.Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new HubException("Message is required.");
            }

            if (receiverId == senderId)
            {
                throw new HubException("Cannot send message to yourself.");
            }

            var senderName =
                Context.User?.FindFirstValue(ClaimTypes.Name)
                ?? Context.User?.FindFirstValue(ClaimTypes.Email)
                ?? "User";

            var message = new
            {
                id = Guid.NewGuid(),
                senderId,
                receiverId,
                senderName,
                content = text,
                createdAt = DateTime.UtcNow
            };

            await Clients.Caller.SendAsync("PrivateMessageReceived", message);
            await Clients.User(receiverId.ToString()).SendAsync("PrivateMessageReceived", message);
        }
    }
}
