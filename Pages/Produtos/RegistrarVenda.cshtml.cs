using LojaApp.Data;
using LojaApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; // Para o SelectList (dropdown)
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic; // Para o List
using System.Linq; // Para o .Select
using System.Threading.Tasks; // Para o Task

namespace LojaApp.Pages.Produtos
{
    public class RegistrarVendaModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RegistrarVendaModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Propriedades para o Formulário ---
        public Produto Produto { get; set; } = default!;

        [BindProperty]
        public int QuantidadeVendida { get; set; } = 1;

        [BindProperty]
        public int? ClienteSelecionadoID { get; set; } // O ID do cliente do dropdown (pode ser nulo)

        [BindProperty]
        public string StatusPagamento { get; set; } = "Pago"; // "Pago" ou "Fiado"

        public SelectList ListaClientes { get; set; } = default!;

        // --- Ação GET (Carrega a página) ---
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) { return NotFound(); }

            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null) { return NotFound(); }

            Produto = produto;

            // Carrega os clientes para o dropdown (e adiciona um item "Anónimo")
            var clientes = await _context.Clientes.ToListAsync();
            var listaComAnonimo = new List<Cliente> { new Cliente { ClienteID = 0, Nome = "Consumidor Final (Anónimo)" } };
            listaComAnonimo.AddRange(clientes);

            ListaClientes = new SelectList(listaComAnonimo, "ClienteID", "Nome");

            return Page();
        }

        // --- Ação POST (Regista a Venda) ---
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) { return NotFound(); }

            var produtoParaVender = await _context.Produtos.FindAsync(id);
            if (produtoParaVender == null) { return NotFound(); }

            if (produtoParaVender.Quantidade < QuantidadeVendida)
            {
                Produto = produtoParaVender;
                var clientes = await _context.Clientes.ToListAsync();
                var listaComAnonimo = new List<Cliente> { new Cliente { ClienteID = 0, Nome = "Consumidor Final (Anónimo)" } };
                listaComAnonimo.AddRange(clientes);
                ListaClientes = new SelectList(listaComAnonimo, "ClienteID", "Nome");

                ModelState.AddModelError(string.Empty, "Stock insuficiente!");
                return Page();
            }

            var novaVenda = new Venda
            {
                ClienteID = (ClienteSelecionadoID == 0) ? null : ClienteSelecionadoID,
                DataVenda = System.DateTime.Now,
                StatusPagamento = StatusPagamento
            };
            _context.Vendas.Add(novaVenda);
            await _context.SaveChangesAsync();

            var novoItemVenda = new ItemVenda
            {
                VendaID = novaVenda.VendaID,
                ProdutoID = produtoParaVender.ProdutoID,
                Quantidade = QuantidadeVendida,
                PrecoUnitario = produtoParaVender.Preco
            };
            _context.ItensVenda.Add(novoItemVenda);

            produtoParaVender.Quantidade = produtoParaVender.Quantidade - QuantidadeVendida;
            _context.Attach(produtoParaVender).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}