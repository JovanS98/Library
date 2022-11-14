using BibliotekaWeb.Core.Repositories.ViewModels;
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
        public async Task BookIsAccepted(string userId, ProcessViewModel process)
        {
            await Clients.User(userId).BookIsAccepted(userId, process);
        }
        public async Task BookIsReturned(string userId, Process process)
        {
            await Clients.User(userId).BookIsReturned(userId, process);
        }

        public async Task AddedReservation(Process process)
        {
            await Clients.All.AddedReservation(process);
        }

        public async Task DeletedReservation(Process process)
        {
            await Clients.All.DeletedReservation(process);
        }
    }
}
