using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LojaApp.Models
{
    [Table("Clientes")]
    public class Cliente
    {
        [Key]
        public int ClienteID { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Telefone { get; set; }

        // Propriedade de Navegação: Um cliente pode ter muitas vendas
        public virtual ICollection<Venda>? Vendas { get; set; }
    }
}