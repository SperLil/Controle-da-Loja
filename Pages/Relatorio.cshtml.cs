using LojaApp.Data;
using LojaApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//  NAMESPACE
namespace LojaApp.Pages
{
    // O NOME DA CLASSE
    public class RelatorioModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RelatorioModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int Mes { get; set; } = DateTime.Now.Month; // Padrão: Mês Atual

        [BindProperty(SupportsGet = true)]
        public int Ano { get; set; } = DateTime.Now.Year; // Padrão: Ano Atual

        [BindProperty(SupportsGet = true)]
        public string TermoBuscaCliente { get; set; } = string.Empty;
        public decimal TotalVendidoNoMes { get; set; }

        public IList<ItemVenda> ItensVendidosDoMes { get; set; } = new List<ItemVenda>();

        public async Task OnGetAsync()
        {
            // Define o início e o fim do mês (baseado no filtro)
            var primeiroDiaDoMes = new DateTime(Ano, Mes, 1);
            // Para garantir que pegamos TODAS as vendas do último dia, 
            // pegamos o primeiro dia do MÊS SEGUINTE.
            var primeiroDiaDoProximoMes = primeiroDiaDoMes.AddMonths(1);

            // Encontra os ITENS vendidos no mês filtrado
            var query = _context.ItensVenda
         .Include(i => i.Produto)
         .Include(i => i.Venda).ThenInclude(v => v.Cliente)
         .Where(i => i.Venda.DataVenda >= primeiroDiaDoMes && i.Venda.DataVenda < primeiroDiaDoProximoMes);

            // 2. APLICA O FILTRO DE NOME DO CLIENTE (SE EXISTIR)
            if (!string.IsNullOrEmpty(TermoBuscaCliente))
            {
                // (i.Venda.Cliente != null) é uma verificação de segurança contra vendas anônimas
                query = query.Where(i => i.Venda.Cliente != null && i.Venda.Cliente.Nome.Contains(TermoBuscaCliente));
            }

            // 3. Executa a consulta
            ItensVendidosDoMes = await query
                .OrderByDescending(i => i.Venda.DataVenda)
                .ToListAsync();

            // 4. Calcula o total vendido
            TotalVendidoNoMes = ItensVendidosDoMes.Sum(item => item.PrecoUnitario * item.Quantidade);
        }
    }
}