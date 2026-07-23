using LojaApp.Data;
using LojaApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LojaApp.Pages.Vendas
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Venda Venda { get; set; } = default!;
        public IList<ItemVenda> ItensVenda { get; set; } = default!;

        // NOVO: Propriedade calculada para ser usada no HTML
        public decimal ValorTotal { get; set; }

        // 1. CARREGA OS DADOS DA VENDA NA TELA
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) { return NotFound(); }

            // Carrega a venda e o cliente
            Venda = await _context.Vendas
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(m => m.VendaID == id);

            if (Venda == null) { return NotFound(); }

            // Carrega os itens vendidos, incluindo o Produto
            ItensVenda = await _context.ItensVenda
                .Include(i => i.Produto)
                .Where(i => i.VendaID == id)
                .ToListAsync();

            // CALCULA O VALOR TOTAL A PARTIR DOS ITENS (para exibição no HTML)
            ValorTotal = ItensVenda.Sum(i => i.PrecoUnitario * i.Quantidade);

            return Page();
        }

        // 2. EXECUTA A EXCLUSÃO E REVERTE O ESTOQUE
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) { return NotFound(); }

            var venda = await _context.Vendas.FindAsync(id);

            if (venda != null)
            {
                // Passo 1: Carregar ItensVenda para reverter o estoque
                var itensParaReverter = await _context.ItensVenda
                    .Include(i => i.Produto)
                    .Where(i => i.VendaID == id)
                    .ToListAsync();

                // Passo 2: Reverter o estoque de cada produto
                foreach (var item in itensParaReverter)
                {
                    if (item.Produto != null)
                    {
                        item.Produto.Quantidade += item.Quantidade;
                        _context.Attach(item.Produto).State = EntityState.Modified;
                    }
                }

                // Passo 3: Remover os Pagamentos da venda
                var pagamentos = await _context.Pagamentos
                    .Where(p => p.VendaID == id)
                    .ToListAsync();
                _context.Pagamentos.RemoveRange(pagamentos);

                // Passo 4: Remover os ItensVenda
                _context.ItensVenda.RemoveRange(itensParaReverter);

                // Passo 5: Remover a Venda
                _context.Vendas.Remove(venda);

                // Passo 6: Salvar tudo
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/Relatorio");
        }
    }
}