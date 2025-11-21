using LojaApp.Data;
using LojaApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LojaApp.Pages.Produtos
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Produto Produto { get; set; } = default!;

        // CARREGA O PRODUTO NA TELA PARA CONFIRMAÇÃO
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produto = await _context.Produtos.FindAsync(id);

            if (produto == null)
            {
                return NotFound();
            }

            Produto = produto;
            return Page();
        }

        // EXECUTA A EXCLUSÃO
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produto = await _context.Produtos.FindAsync(id);

            if (produto != null)
            {
                // ANTES DE EXCLUIR O PRODUTO, TEMOS QUE VERIFICAR SE ELE ESTÁ EM VENDAS
                // (Não podemos excluir um produto que já foi vendido, pois quebraria o relatório)
                // (Para um sistema real, nós o marcaríamos como "Inativo", mas por agora, vamos apenas verificar)

                var itemVendido = await _context.ItensVenda.AnyAsync(i => i.ProdutoID == produto.ProdutoID);
                if (itemVendido)
                {
                    // Se já foi vendido, não pode excluir. Recarrega a página com erro.
                    ModelState.AddModelError(string.Empty, "Não é possível excluir este produto, pois ele já possui vendas registradas. Considere zerar o estoque.");
                    Produto = produto;
                    return Page();
                }

                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}