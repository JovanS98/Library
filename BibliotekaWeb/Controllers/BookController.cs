using BibliotekaWeb.Core;
using BibliotekaWeb.Core.Repositories.ViewModels;
using BibliotekaWeb.Data;
using BibliotekaWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using System.Net;
using System.Security.Claims;

namespace BibliotekaWeb.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _db;

        
        public BookController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Authorize(Roles = $"{Constants.Roles.Administrator}")]
        public IActionResult AddBook()
        {
            return View();
        }

        public IActionResult Library()
        {

            IEnumerable<Book> books = _db.Books.ToList();
            List<BookWithNumViewModel> booksWithNum = new List<BookWithNumViewModel>();

            var userId = getCurrentUserId();
            var userReservations = getAllReservationsFromCurrentUser();
            // Ispitujem da li je korisnik presao granicu od 5 rezervisanih knjiga
            // Ovde imam bag jer sam racunao samo knjige koje imam rezervisane, nisam racunao na "nerazresene", i onda moze da se posalje
            // zahtev za vise knjiga odjednom i ako admin prihvati onda ce korisnik imati vise od dozvoljenog limita
            bool aboveTheLimit = userReservations.Count() > Constants.Limits.ReservationLimit-1 ? true : false;

            foreach (var book in books)
            {
                var bookId = book.BookId;

                Func<Process, bool> condition = p => p.BookId == bookId;

                Process? process = _db.Processes
                    .Include(b => b.Book)
                    .OrderBy(t => t.Time)
                    .Where(p => p.PendingReservation == false)
                    .LastOrDefault(condition);

                if (process == null)
                {
                    return NotFound();
                }

                BookWithNumViewModel b = new BookWithNumViewModel();
                b.Book.BookId = process.BookId;
                b.Book.Name = process.Book.Name;
                b.Book.Author = process.Book.Author;
                b.NumberOfAvailableCopies = process.NumberOfAvailableCopies;

                // Prvo sam uradio bez sledecih linija, ali sam shvatio da moram nekako da znam da li je korisnik vec
                // rezervisao knjigu ili je rezervacija na cekanju, u tom slucaju se blokira dugme za rezervaciju na neki nacin.
                // Ako nije ni jedno ni drugo, znaci da korisnik moze da zatrazi rezervaciju ako ima manje od 5 knjiga
                // Lose sam uradio ovo zato sto sam tek na kraju skapirao sta mi sve treba, bazu sam mogao da modelujem drugacije
                // da bi bilo lakse sve ovo da se odradi

                condition = p => p.BookId == bookId && p.UserId == userId;

                process = _db.Processes
                    .Include(b => b.Book)
                    .OrderBy(t => t.Time)
                    .LastOrDefault(condition);

                if (process != null)
                {
                    if (process.IsActiveReservation)
                        b.Status = Enums.ActiveStatus.reserved;
                    else if (process.PendingReservation && process.Status == Enums.Status.reserved)
                        b.Status = Enums.ActiveStatus.pendingForReserve;
                    else if (process.PendingReservation && process.Status == Enums.Status.returned)
                        b.Status = Enums.ActiveStatus.pendingForReturn;
                    else if (process.NumberOfAvailableCopies == 0)
                        b.Status = Enums.ActiveStatus.noAvailableBooks;
                    else if (aboveTheLimit)
                        b.Status = Enums.ActiveStatus.aboveTheLimit;
                    else 
                        b.Status = Enums.ActiveStatus.nothing;
                }
                else
                {
                    if (aboveTheLimit)
                        b.Status = Enums.ActiveStatus.aboveTheLimit;
                    else
                        b.Status = Enums.ActiveStatus.nothing;
                }

                booksWithNum.Add(b);
            }

            return View(booksWithNum);
        }


        [HttpPost]
        [Authorize(Roles = $"{Constants.Roles.Administrator}")]
        [ValidateAntiForgeryToken]
        public IActionResult AddBook(BookWithNumViewModel obj)
        {
            // Postavljam novu knjigu
            _db.Books.Add(obj.Book);

            _db.SaveChanges();

            var userId = getCurrentUserId();

            Process process = new Process();

            // Medjutim, knjiga dobija id tek kada se upise u bazu, sto je trenutno nevidljivo
            // Moram da nadjem id npr preko naziva, ne pada mi na pamet drugi nacin

            var book = _db.Books.First(b => b.Name == obj.Book.Name);

            var bookId = book.BookId;

            process.UserId = userId;
            process.BookId = bookId;
            process.Time = DateTime.Now;
            process.Status = Enums.Status.added;
            process.ReturnDeadline = process.Time.AddDays(30);
            process.IsActiveReservation = false;
            process.PendingReservation = false;
            process.NumberOfAvailableCopies = obj.NumberOfAvailableCopies;

            _db.Processes.Add(process);
            _db.SaveChanges();

            return RedirectToAction("Library");
        }

        public string getCurrentUserId()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string userId;

            if(claims != null)
            {
                userId = claims.Value;
            }
            else
            {
                userId = null;
            }

            return userId;
        }

        // Ova funkcija vraca i aktivne rezervacije i one koje su na cekanju
        // Koristim je za proveru limita knjiga
        public List<Process> getAllReservationsFromCurrentUser()
        {
            var userId = getCurrentUserId();

            Func<Process, bool> condition = p => p.UserId == userId && (p.IsActiveReservation == true || (p.PendingReservation == true && p.Status == Enums.Status.reserved));

            List<Process> objReservationsList = _db.Processes.Include(b => b.Book).Where(condition).ToList();

            return objReservationsList;
        }
    }
}
