using LojaApp.Data;
using LojaApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LojaApp.Pages
{
    public class RelatorioModel : PageModel
    {
        public decimal TotalRecebidoNoMes { get; set; }
        private readonly ApplicationDbContext _context;

        public RelatorioModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int Mes { get; set; } = DateTime.Now.Month;

        [BindProperty(SupportsGet = true)]
        public int Ano { get; set; } = DateTime.Now.Year;

        [BindProperty(SupportsGet = true)]
        public string TermoBuscaCliente { get; set; } = string.Empty;

        public decimal TotalVendidoNoMes { get; set; }

        public IList<ItemVenda> ItensVendidosDoMes { get; set; } = new List<ItemVenda>();

        public async Task OnGetAsync()
        {
            var primeiroDiaDoMes = new DateTime(Ano, Mes, 1);
            var primeiroDiaDoProximoMes = primeiroDiaDoMes.AddMonths(1);

            var query = _context.ItensVenda
                .Include(i => i.Produto)
                .Include(i => i.Venda)
                    .ThenInclude(v => v.Cliente)
                .Include(i => i.Venda)
                    .ThenInclude(v => v.Pagamentos)
                .Where(i => i.Venda.DataVenda >= primeiroDiaDoMes
                         && i.Venda.DataVenda < primeiroDiaDoProximoMes);

            if (!string.IsNullOrEmpty(TermoBuscaCliente))
            {
                query = query.Where(i => i.Venda.Cliente != null
                                      && i.Venda.Cliente.Nome.Contains(TermoBuscaCliente));
            }

            ItensVendidosDoMes = await query
                .OrderByDescending(i => i.Venda.DataVenda)
                .ToListAsync();

            TotalVendidoNoMes = ItensVendidosDoMes
                .Sum(item => item.PrecoUnitario * item.Quantidade);

            TotalRecebidoNoMes = await _context.Pagamentos
                .Where(p => p.DataPagamento >= primeiroDiaDoMes
                         && p.DataPagamento < primeiroDiaDoProximoMes)
                .SumAsync(p => p.ValorPago);
        }
    }
}