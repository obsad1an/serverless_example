using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BroadcasterService.Hubs
{
    public class ChatHub : Hub
    {
        public async Task BroadcastMessage(string name, string message)
        {
            await Clients.Group("test1").SendAsync("broadcastMessage", name, message);
        }

        public async override Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}