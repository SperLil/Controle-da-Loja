using LojaApp.Data;
using LojaApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LojaApp.Pages.Vendas
{
    public class RegistarPagamentoModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RegistarPagamentoModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Venda Venda { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public decimal ValorPago { get; set; }

        [BindProperty]
        public string FormaPagamento { get; set; } = "Dinheiro";

        [BindProperty]
        public string? Observacao { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Id == 0) return NotFound();

            Venda = await _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Pagamentos)
                .Include(v => v.ItensVenda)
                .FirstOrDefaultAsync(v => v.VendaID == Id);

            if (Venda == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Venda = await _context.Vendas
                .Include(v => v.Pagamentos)
                .FirstOrDefaultAsync(v => v.VendaID == Id);

            if (Venda == null) return NotFound();

            if (ValorPago <= 0 || ValorPago > Venda.ValorRestante)
            {
                ModelState.AddModelError("ValorPago",
                    $"O valor deve ser entre R$0,01 e R${Venda.ValorRestante:F2}.");
                return Page();
            }

            var pagamento = new Pagamento
            {
                VendaID = Venda.VendaID,
                ValorPago = ValorPago,
                FormaPagamento = FormaPagamento,
                Observacao = Observacao,
                DataPagamento = DateTime.Now
            };

            _context.Pagamentos.Add(pagamento);
            await _context.SaveChangesAsync();

            var totalPagoAtualizado = await _context.Pagamentos
                .Where(p => p.VendaID == Venda.VendaID)
                .SumAsync(p => p.ValorPago);

            Venda.StatusPagamento = totalPagoAtualizado >= Venda.ValorTotal ? "Pago"
                                  : totalPagoAtualizado > 0 ? "Parcial"
                                                                           : "Fiado";

            await _context.SaveChangesAsync();

            return RedirectToPage("/Relatorio");
        }
    }
}