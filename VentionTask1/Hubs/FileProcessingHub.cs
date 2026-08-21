using Microsoft.AspNetCore.SignalR;

namespace VentionTask1.WebApi.Hubs
{
    public class FileProcessingHub : Hub
    {
        public Task JoinOrganizationFilesGroup(Guid organizationId)
        {
            return Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"org-{organizationId}-files");
        }

        public Task LeaveOrganizationFilesGroup(Guid organizationId)
        {
            return Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"org-{organizationId}-files");
        }
    }
}
