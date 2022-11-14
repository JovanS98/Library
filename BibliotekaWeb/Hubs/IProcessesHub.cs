using BibliotekaWeb.Core.Repositories.ViewModels;
using BibliotekaWeb.Models;

namespace BibliotekaWeb.Hubs
{
    public interface IProcessesHub
    {
        Task NewProcessReceived(Process newProcess);

        Task BookIsAccepted(string userId, ProcessViewModel process);

        Task BookIsReturned(string userId, Process process);

        Task AddedReservation(Process process);

        Task DeletedReservation(Process process);
    }
}
