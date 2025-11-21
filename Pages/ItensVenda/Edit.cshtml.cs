using LojaApp.Data;
using LojaApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LojaApp.Pages.ItensVenda
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ItemVenda ItemVenda { get; set; } = default!;

        // Propriedades para mostrar os detalhes no HTML
        public Produto ProdutoOriginal { get; set; } = default!;
        public Venda VendaOriginal { get; set; } = default!;

        // CARREGA O ITEM DA VENDA PARA EDIÇÃO
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) { return NotFound(); }

            // A CORREÇÃO ESTÁ AQUI: TEMOS DE USAR .Include()
            ItemVenda = await _context.ItensVenda
                .Include(i => i.Produto) // <-- Carrega o Produto
                .Include(i => i.Venda).ThenInclude(v => v.Cliente) // <-- Carrega a Venda e o Cliente
                .FirstOrDefaultAsync(m => m.ItemVendaID == id);

            if (ItemVenda == null) { return NotFound(); }

            // Preenche as propriedades para o HTML usar
            // Se o ItemVenda.Produto for nulo (produto apagado), o erro acontece aqui
            if (ItemVenda.Produto == null || ItemVenda.Venda == null)
            {
                // Tratar o caso de um produto ou venda ter sido apagado
                return NotFound();
            }

            ProdutoOriginal = ItemVenda.Produto;
            VendaOriginal = ItemVenda.Venda;

            return Page();
        }

        // SALVA AS ALTERAÇÕES DO ITEM
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Se o modelo falhar, temos de recarregar os dados do OnGet
                return await OnGetAsync(ItemVenda.ItemVendaID);
            }

            var itemAntigo = await _context.ItensVenda
                .Include(i => i.Venda)
                .Include(i => i.Produto)
                .FirstOrDefaultAsync(i => i.ItemVendaID == ItemVenda.ItemVendaID);

            if (itemAntigo == null) { return NotFound(); }

            var quantidadeAntiga = itemAntigo.Quantidade;
            var produtoRelacionado = itemAntigo.Produto;
            var vendaRelacionada = itemAntigo.Venda;

            // Verifica se o produto ainda existe
            if (produtoRelacionado == null || vendaRelacionada == null)
            {
                ModelState.AddModelError(string.Empty, "Produto ou Venda original não encontrado.");
                return await OnGetAsync(ItemVenda.ItemVendaID);
            }

            itemAntigo.Quantidade = ItemVenda.Quantidade;
            itemAntigo.PrecoUnitario = ItemVenda.PrecoUnitario;

            produtoRelacionado.Quantidade += quantidadeAntiga;
            produtoRelacionado.Quantidade -= itemAntigo.Quantidade;

            _context.Attach(itemAntigo).State = EntityState.Modified;
            _context.Attach(produtoRelacionado).State = EntityState.Modified;
            _context.Attach(vendaRelacionada).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return RedirectToPage("/Relatorio");
        }
    }
}