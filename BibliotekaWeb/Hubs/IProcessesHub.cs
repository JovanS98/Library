using BibliotekaWeb.Models;

namespace BibliotekaWeb.Hubs
{
    public interface IProcessesHub
    {
        Task NewProcessReceived(Process newProcess);
    }
}
