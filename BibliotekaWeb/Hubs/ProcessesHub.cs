using BibliotekaWeb.Models;
using Microsoft.AspNetCore.SignalR;

namespace BibliotekaWeb.Hubs
{
    public class ProcessesHub : Hub<IProcessesHub>
    {
        public async Task NewProcessReceived(Process newProcess)
        {
            await Clients.All.NewProcessReceived(newProcess);
        }
        public async Task BookIsAccepted(string userId, Process process)
        {
            await Clients.User(userId).BookIsAccepted(userId, process);
        }

        public string GetConnectionId() => Context.ConnectionId;
    }
}
