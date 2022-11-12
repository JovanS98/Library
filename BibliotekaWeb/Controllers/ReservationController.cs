﻿using BibliotekaWeb.Core;
using BibliotekaWeb.Core.Repositories.ViewModels;
using BibliotekaWeb.Data;
using BibliotekaWeb.Hubs;
using BibliotekaWeb.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static BibliotekaWeb.Core.Constants;
using AuthorizeAttribute = Microsoft.AspNetCore.Authorization.AuthorizeAttribute;

namespace BibliotekaWeb.Controllers
{
    
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHubContext<ProcessesHub, IProcessesHub> _hubContext;

        public ReservationController(ApplicationDbContext db, IHubContext<ProcessesHub, IProcessesHub> hubContext)
        {
            _db = db;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = $"{Constants.Roles.Administrator}")]
        public IActionResult AllReservations()
        {
            List<Process> objReservationsList = _db.Processes
                                                .Include(b => b.Book)
                                                .Include(u => u.User)
                                                .OrderBy(o => o.UserId)
                                                .Where(p => p.IsActiveReservation == true)
                                                .ToList();

            return View(objReservationsList);
        }

        // Ovo je za koriscenje signalR  
          [HttpGet]
          [Route("api/processes")]
          [Authorize(Roles = $"{Constants.Roles.Administrator}")]
          public IActionResult Get()
          {
              List<Process> objReservationsList = _db.Processes
                                                  .Include(b => b.Book)
                                                  .Include(u => u.User)
                                                  .OrderBy(o => o.UserId)
                                                  .Where(p => p.PendingReservation == true)
                                                  .ToList();

              return Ok(objReservationsList);
          } 

        [Authorize(Roles = $"{Constants.Roles.Administrator}")]
        public IActionResult PendingReservations()
        {
            List<Process> objReservationsList = _db.Processes
                                                  .Include(b => b.Book)
                                                  .Include(u => u.User)
                                                  .OrderBy(o => o.UserId)
                                                  .Where(p => p.PendingReservation == true)
                                                  .ToList();

            return View(objReservationsList);
        } 

        [Authorize(Roles = $"{Constants.Roles.User}")]
        public IActionResult UserReservations()
        {
            IEnumerable<Process> objReservationsList = getCurrentUserReservations();
            List<ProcessViewModel> pwmList = new List<ProcessViewModel>();

            // Prolazim kroz sve rezervacije da proverim da li postoji zatrazen povrat knjige
            // tj. trazim da li je korisnik kliknuo na "return the book"
            foreach(Process objReservation in objReservationsList)
            {
                var userId = objReservation.UserId;
                var bookId = objReservation.BookId;

                Func<Process, bool> condition = p => p.BookId == bookId && p.UserId == userId && p.Status == Enums.Status.returned;

                Process? process = _db.Processes.Where(p => p.PendingReservation == true).FirstOrDefault(condition);

                ProcessViewModel pwm = new ProcessViewModel();
                pwm.Process = objReservation;

                if(process != null)
                {
                    pwm.PendingForReturn = true;
                }

                pwmList.Add(pwm);
            }

            return View(pwmList);
        }

        // Ne radi sa httpPost
        // A mozda mi signalR ne radi jer nema http tag?
        // [HttpPost]
        public async Task<IActionResult> ReserveBook(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var userId = getCurrentUserId();

            Process process = new Process();

            // Ovde sam shvatio da sam mogao u enum status da ubacim jos jedno stanje, nesto kao "nerazreseno" za one 
            // rezervacije koje su na cekanju, ali sam ranije ubacio polje "pending reservation" koje pokazuje da li je na cekanju
            // i nisam imao vremena da menjam sve u bazi, kao difoltan status stavljam "reserved"
            // i posle kada admin prihvati, pending se menja u false i reserved ostaje, mozda nije najbolje resenje, ali tako sam prvo uradio

            process.UserId = userId;
            process.BookId = (int)id;
            process.Status = Enums.Status.reserved;
            process.ReturnDeadline = process.Time.AddDays(Constants.Limits.Deadline);
            process.IsActiveReservation = false;
            process.PendingReservation = true;

            //stavljam 0 kao difoltno jer nije bitno trenutno, a stavio sam da je not null pa moram da imam nesto, a nisam 
            //podesavao difoltne vrednosti u klasama(verovatno je trebalo to da uradim)
            process.NumberOfAvailableCopies = 0;

            _db.Processes.Add(process);
            await _db.SaveChangesAsync();

            await _hubContext.Clients.All.NewProcessReceived(process);

            return RedirectToAction("Library", "Book");
        }

        // Ne radi sa httpPost
        //[HttpPost]
        public async Task<IActionResult> ReturnBook(int? id)
        {

             if (id == null || id == 0)
            {
                return NotFound();
            } 

            var userId = getCurrentUserId();

            Process process = new Process();

            process.UserId = userId;
            process.BookId = (int)id;
            process.Status = Enums.Status.returned;
            process.ReturnDeadline = process.Time.AddDays(Constants.Limits.Deadline);
            process.IsActiveReservation = false;
            process.PendingReservation = true;
            process.NumberOfAvailableCopies = 0;

            _db.Processes.Add(process);
            await _db.SaveChangesAsync();

            await _hubContext.Clients.All.NewProcessReceived(process);

            return RedirectToAction("UserReservations");
        }

        public IActionResult MarkReturnedBook(string userId, int bookId, DateTime time)
        {
            Func<Process, bool> condition = p => p.UserId == userId && p.BookId == bookId && p.Time.Date.Equals(time.Date);

            Process? updateProcess = _db.Processes.Where(p => p.PendingReservation == true).FirstOrDefault(condition);

            if (updateProcess == null)
            {
                return NotFound();
            }

            updateProcess.PendingReservation = false;

            // Trazim broj preostalih primeraka, bitno je da nije "nerazresena" rezervacija
            condition = p => p.BookId == bookId;
            Process? numProcess = _db.Processes.OrderBy(o => o.Time).Where(p => p.PendingReservation == false).LastOrDefault(condition);

            if (numProcess == null)
            {
                return NotFound();
            }

            updateProcess.NumberOfAvailableCopies = numProcess.NumberOfAvailableCopies + 1;

            // Azuriram proces da se zna da je admin prihvatio povrat knjige
            _db.Processes.Update(updateProcess);
            _db.SaveChanges();

            // Sada trazim proces u kome stoji da je aktivna rezervacija, treba da je promenim na neaktivnu
            // Nisam zeleo da menjam procese iz stanja u stanje kako bih sacuvao informacije o tome ko je kad koju knjigu rezervisao i kad je vratio

            condition = p => p.BookId == bookId && p.UserId == userId;
            Process? activeProcess = _db.Processes.OrderBy(o => o.Time).Where(p => p.IsActiveReservation == true).LastOrDefault(condition);

            if (activeProcess == null)
            {
                return NotFound();
            }

            activeProcess.IsActiveReservation = false;

            // Azuriram proces koji govori da korisnik vise nema ovu knjigu
            _db.Processes.Update(activeProcess);
            _db.SaveChanges();

            return RedirectToAction("PendingReservations");
        }

        public IActionResult AcceptReservation(string userId, int bookId, DateTime time)
        {
            //Hocu da dohvatim proces sa poslatim podacima preko viewa(probao sam da prebacim ceo objekat, ali se nije lepo slao
            //Moguce da ne moze ceo objekat pa sam morao posebna 3 podatka da posaljem
            //Pretpostavljam da preko Find() moze da se posalje primarni kljuc i tako nadje proces, medjutim 
            //ne znam sintaksu za slozeni kljuc pa sam pravio condition gde ispitujem sva 3 podatka posebno

            Func<Process, bool> condition = p => p.UserId == userId && p.BookId == bookId && p.Time.Date.Equals(time.Date);

            Process? updateProcess = _db.Processes.Where(p => p.PendingReservation == true).FirstOrDefault(condition);

            if (updateProcess == null)
            {
                return NotFound();
            }

            updateProcess.PendingReservation = false;
            updateProcess.IsActiveReservation = true;

            //Posto cuvam kolonu Time koja predstavlja vreme kada je zatrazena rezervacija, dodacu npr. 30 dana da traje
            //iznajmljivanje, ali na trenutno vreme, a ne na prvobitnu rezervaciju, jer ako npr. admin ceo dan nije potvrdio
            //rezervaciju, onda bi u onom slucaju korisnik imao dan manje
            updateProcess.ReturnDeadline = DateTime.Now.AddDays(Constants.Limits.Deadline);

            //Zamislio sam u bazi da broj primeraka cuvam u procesima koji nemaju oznaku "pending reservation" tako da moram da 
            //dohvatim poslednji proces u kome je ukljucena ta knjiga, a da nije "nerazresena", znaci ili da je dodata ili
            //rezervisana ili vracena

            //Ovo ne radi, ne znam zasto
            // condition = p => p.BookId == bookId && p.PendingReservation == false
            // Process? numProcess = _db.Processes.OrderBy(o => o.Time).LastOrDefault(condition);

            //Ovo radi
            condition = p => p.BookId == bookId;
            Process? numProcess = _db.Processes.OrderBy(o => o.Time).Where(p => p.PendingReservation == false).LastOrDefault(condition);

            if (numProcess == null)
            {
                return NotFound();
            }

            //Nakon sto sam dohvatio poslednji proces kada se menjao broj primeraka za knjigu, sada smanjujem za 1 
            //i dopustam rezervaciju(apdejtujem proces)

            updateProcess.NumberOfAvailableCopies = numProcess.NumberOfAvailableCopies - 1;

            _db.Processes.Update(updateProcess);
            _db.SaveChanges();

            return RedirectToAction("PendingReservations");
        }

        public string getCurrentUserId()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string userId;

            if (claims != null)
            {
                userId = claims.Value;
            }
            else
            {
                userId = null;
            }

            return userId;
        }

        // Ova funkcija vraca sve aktivne rezervacije trenutnog korisnika
        public List<Process> getCurrentUserReservations()
        {
            var userId = getCurrentUserId();

            Func<Process, bool> condition = p => p.UserId == userId && p.IsActiveReservation == true;

            List<Process> objReservationsList = _db.Processes.Include(b => b.Book).Where(condition).ToList();

            return objReservationsList;
        }
    }
}
