using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LojaApp.Models
{
    [Table("ItensVenda")]
    public class ItemVenda
    {
        [Key]
        public int ItemVendaID { get; set; }

        public int VendaID { get; set; } // O "Recibo"

        public int ProdutoID { get; set; } // O Produto

        public int Quantidade { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal PrecoUnitario { get; set; }

        // --- Propriedades de Navegação ---
        [ForeignKey("VendaID")]
        public virtual Venda? Venda { get; set; }

        [ForeignKey("ProdutoID")]
        public virtual Produto? Produto { get; set; }
    }
}