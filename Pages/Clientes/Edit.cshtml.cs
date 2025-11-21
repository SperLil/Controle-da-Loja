using LojaApp.Data;
using LojaApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LojaApp.Pages.Clientes
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Cliente Cliente { get; set; } = default!;

        // CARREGA O CLIENTE PARA EDIÇÃO
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) { return NotFound(); }
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) { return NotFound(); }

            Cliente = cliente;
            return Page();
        }

        // SALVA AS ALTERAÇÕES
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) { return Page(); }

            _context.Attach(Cliente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Clientes.Any(e => e.ClienteID == Cliente.ClienteID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }
    }
}