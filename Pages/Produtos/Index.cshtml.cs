using LojaApp.Data;
using LojaApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LojaApp.Pages.Produtos
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Produto> Produto { get; set; } = new List<Produto>();

        [BindProperty(SupportsGet = true)]
        public string TermoBusca { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            var query = from p in _context.Produtos
                        select p;

            if (!string.IsNullOrEmpty(TermoBusca))
            {
                query = query.Where(p => p.Nome.Contains(TermoBusca));
            }

            Produto = await query.ToListAsync();
        }
    }
}