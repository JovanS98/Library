using BibliotekaWeb.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using static BibliotekaWeb.Core.Enums;

namespace BibliotekaWeb.Models
{
    public class Process
    {
        public string UserId { get; set; }
        public int BookId { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        [Required]
        public Status Status { get; set; }
        public DateTime ReturnDeadline { get; set; }
        [Required]
        public bool IsActiveReservation { get; set; }
        [Required]
        public bool PendingReservation { get; set; }
        [Required]
        public int NumberOfAvailableCopies { get; set; }

        public Book Book { get; set; }
        public ApplicationUser User { get; set; }

    }
}
