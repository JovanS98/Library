using BibliotekaWeb.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace BibliotekaWeb.Core.Repositories
{
    public interface IRoleRepository
    {
        ICollection<IdentityRole> GetRoles();
    }
}
