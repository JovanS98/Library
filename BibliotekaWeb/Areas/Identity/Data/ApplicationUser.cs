using Microsoft.AspNetCore.Identity;

namespace BibliotekaWeb.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Adress { get; set; }

        public ApplicationUser()
        {
            FirstName = "Default";
            LastName = "Default";
            Adress = "Default";
        }
    }

    public class ApplicationRole : IdentityRole
    {

    }
}
