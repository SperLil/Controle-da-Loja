using LojaApp.Data;
using LojaApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LojaApp.Pages
{
    // ESTA É A CLASSE QUE ESTAVA FALTANDO
    public class RelatorioFiadoModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RelatorioFiadoModel(ApplicationDbContext context)
        {
            _context = context;
        }
        [BindProperty(SupportsGet = true)]
        public string TermoBuscaCliente { get; set; } = string.Empty;
        public IList<Venda> VendasFiado { get; set; } = new List<Venda>();
        public decimal TotalFiado { get; set; }

        public async Task OnGetAsync()
        {
            // 1. Procura todas as vendas com Status "Fiado"
            var query = _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.ItensVenda).ThenInclude(item => item.Produto)
                .Where(v => v.StatusPagamento == "Fiado");

            // 2. APLICA O FILTRO DE NOME DO CLIENTE (SE EXISTIR)
            if (!string.IsNullOrEmpty(TermoBuscaCliente))
            {
                // (v.Cliente != null) é uma verificação de segurança
                query = query.Where(v => v.Cliente != null && v.Cliente.Nome.Contains(TermoBuscaCliente));
            }

            // 3. Executa a consulta
            VendasFiado = await query
                .OrderBy(v => v.Cliente.Nome) // Ordena por nome
                .ThenBy(v => v.DataVenda)     // E depois por data
                .ToListAsync();

            // 4. Calcula o total que os clientes devem
            TotalFiado = 0;
            foreach (var venda in VendasFiado)
            {
                TotalFiado += venda.ItensVenda?.Sum(item => item.PrecoUnitario * item.Quantidade) ?? 0;
            }
        }

        public async Task<IActionResult> OnPostMarcarPagoAsync(int vendaId)
        {
            var venda = await _context.Vendas.FindAsync(vendaId);

            if (venda != null)
            {
                venda.StatusPagamento = "Pago";
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}