using Microsoft.AspNetCore.SignalR;

namespace VentionTask1.WebApi.Hubs
{
    public class FileProcessingHub : Hub
    {
        public async Task WatchFiles(IEnumerable<Guid> fileIds)
        {
            foreach (var fileId in fileIds.Distinct())
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    GetFileGroupName(fileId));
            }
        }

        public async Task UnwatchFiles(IEnumerable<Guid> fileIds)
        {
            foreach (var fileId in fileIds.Distinct())
            {
                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    GetFileGroupName(fileId));
            }
        }

        private static string GetFileGroupName(Guid fileId)
        {
            return $"file-{fileId}";
        }
    }
}
