using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LojaApp.Models
{
    public class Pagamento
    {
        [Key]
        public int PagamentoID { get; set; }

        [Required]
        public int VendaID { get; set; }

        [Required]
        [Display(Name = "Data do Pagamento")]
        public DateTime DataPagamento { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor pago deve ser maior que zero.")]
        [Display(Name = "Valor Pago")]
        public decimal ValorPago { get; set; }

        // "Dinheiro" | "PIX" | "Cartão" | etc.
        [MaxLength(50)]
        [Display(Name = "Forma de Pagamento")]
        public string FormaPagamento { get; set; } = "Dinheiro";

        [MaxLength(255)]
        [Display(Name = "Observação")]
        public string? Observacao { get; set; }

        // Navegação
        public Venda? Venda { get; set; }
    }
}