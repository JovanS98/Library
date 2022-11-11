using BibliotekaWeb.Models;

namespace BibliotekaWeb.Core.Repositories.ViewModels
{
    public class ProcessViewModel
    {
        public Process Process { get; set; }

        public bool PendingForReturn { get; set; }

        public ProcessViewModel()
        {
            Process = new Process();
            PendingForReturn = false;
        }
    }
}
