using BibliotekaWeb.Areas.Identity.Data;
using BibliotekaWeb.Core.Repositories;
using BibliotekaWeb.Data;
using Microsoft.AspNetCore.Identity;

namespace BibliotekaWeb.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _context;
        public RoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public ICollection<IdentityRole> GetRoles()
        {
            return _context.Roles.ToList();
        }
    }
}
