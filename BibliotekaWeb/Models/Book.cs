using System.ComponentModel.DataAnnotations;

namespace BibliotekaWeb.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Author { get; set; }

    }
}
