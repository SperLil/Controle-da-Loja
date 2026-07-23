using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace LojaApp.Models
{
    [Table("Vendas")]
    public class Venda
    {
        [Key]
        public int VendaID { get; set; }

        public DateTime DataVenda { get; set; }

        public int? ClienteID { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ValorTotal { get; set; }

        [StringLength(50)]
        public string StatusPagamento { get; set; } = "Pago";

        [ForeignKey("ClienteID")]
        public virtual Cliente? Cliente { get; set; }

        public virtual ICollection<ItemVenda>? ItensVenda { get; set; }

        public virtual ICollection<Pagamento>? Pagamentos { get; set; }

        [NotMapped]
        public decimal TotalPago => Pagamentos?.Sum(p => p.ValorPago) ?? 0;

        [NotMapped]
        public decimal ValorRestante => ValorTotal - TotalPago;

        [NotMapped]
        public bool EmAberto => ValorRestante > 0;
    }
}