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
        public decimal TotalEmAberto { get; set; }

        public async Task OnGetAsync()
        {
            // Busca vendas "Fiado" OU "Parcial" — ambas têm saldo em aberto
            var query = _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.ItensVenda).ThenInclude(item => item.Produto)
                .Include(v => v.Pagamentos) // <-- necessário para ValorRestante
                .Where(v => v.StatusPagamento == "Fiado"
                         || v.StatusPagamento == "Parcial");

            if (!string.IsNullOrEmpty(TermoBuscaCliente))
            {
                query = query.Where(v => v.Cliente != null
                                      && v.Cliente.Nome.Contains(TermoBuscaCliente));
            }

            VendasFiado = await query
                .OrderBy(v => v.Cliente != null ? v.Cliente.Nome : "")
                .ThenBy(v => v.DataVenda)
                .ToListAsync();

            // Total real em aberto (usando ValorRestante de cada venda)
            TotalEmAberto = VendasFiado.Sum(v => v.ValorRestante);
        }

        public async Task<IActionResult> OnPostMarcarPagoAsync(int vendaId)
        {
            var venda = await _context.Vendas
                .Include(v => v.Pagamentos)
                .FirstOrDefaultAsync(v => v.VendaID == vendaId);

            if (venda != null)
            {
                // Regista pagamento do restante
                var restante = venda.ValorRestante;
                if (restante > 0)
                {
                    _context.Pagamentos.Add(new Pagamento
                    {
                        VendaID = venda.VendaID,
                        ValorPago = restante,
                        FormaPagamento = "Dinheiro",
                        Observacao = "Acerto total do fiado",
                        DataPagamento = DateTime.Now
                    });
                }

                venda.StatusPagamento = "Pago";
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}