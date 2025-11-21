using LojaApp.Data;
using LojaApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LojaApp.Pages.Clientes
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Cliente Cliente { get; set; } = default!;

        // 1. CARREGA OS DADOS NA TELA
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) { return NotFound(); }
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) { return NotFound(); }

            Cliente = cliente;
            return Page();
        }

        // 2. EXECUTA A EXCLUSÃO
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) { return NotFound(); }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                // Verifica se o cliente tem vendas pendentes (boa prática)
                var vendasPendentes = await _context.Vendas.AnyAsync(v => v.ClienteID == cliente.ClienteID);
                if (vendasPendentes)
                {
                    // NÃO PODE APAGAR clientes que têm fiado ou vendas registradas!
                    ModelState.AddModelError(string.Empty, "Não é possível excluir o cliente: ele possui vendas registradas.");
                    // Recarrega a página para mostrar o erro (precisamos do OnGet para recarregar o Cliente)
                    return await OnGetAsync(id);
                }

                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}