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

        [StringLength(50)]
        public string StatusPagamento { get; set; } = "Pago";

        // --- Propriedades de Navegação ---
        [ForeignKey("ClienteID")]
        public virtual Cliente? Cliente { get; set; }

        // UMA Venda agora tem VÁRIOS ItensVenda
        public virtual ICollection<ItemVenda>? ItensVenda { get; set; }
    }
}
