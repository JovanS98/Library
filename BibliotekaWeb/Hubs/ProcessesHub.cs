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
    }
}
