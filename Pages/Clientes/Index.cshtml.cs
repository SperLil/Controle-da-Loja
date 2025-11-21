using LojaApp.Data;
using LojaApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LojaApp.Pages.Clientes
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Cliente> Cliente { get; set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Clientes != null)
            {
                Cliente = await _context.Clientes.ToListAsync();
            }
        }
    }
}