using BibliotekaWeb.Models;
using static BibliotekaWeb.Core.Enums;

namespace BibliotekaWeb.Core.Repositories.ViewModels
{
    public class BookWithNumViewModel
    {
        public Book Book { get; set; }

        public int NumberOfAvailableCopies { get; set; }

        public ActiveStatus Status;

        public BookWithNumViewModel()
        {
            Book = new Book();
            NumberOfAvailableCopies = 0;
            Status = ActiveStatus.nothing;
        }
    }
}
