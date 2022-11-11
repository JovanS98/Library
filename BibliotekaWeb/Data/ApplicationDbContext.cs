using BibliotekaWeb.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BibliotekaWeb.Data
{
    public class ApplicationDbContext : BibliotekaWebContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books {  get; set; } 
        public DbSet<Process> Processes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Process>().HasKey(x => new { x.UserId, x.BookId, x.Time });

        }
    }
}
