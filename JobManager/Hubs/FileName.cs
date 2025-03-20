using Microsoft.AspNetCore.SignalR;

namespace JobManager.Endpoints
{
    public class JobHub : Hub
    {
        public async Task SubscribeToJobUpdates()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "JobUpdates");
        }

        public async Task UnsubscribeFromJobUpdates()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "JobUpdates");
        }
    }
}