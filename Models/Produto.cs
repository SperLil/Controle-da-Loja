using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LojaApp.Models
{
    [Table("Produtos")]
    public class Produto
    {
        [Key]
        public int ProdutoID { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)] // Boa prática
        public string Nome { get; set; } = string.Empty; // <-- ISSO CORRIGE O AVISO CS8618

        [Column(TypeName = "decimal(10, 2)")]
        [Required(ErrorMessage = "O preço é obrigatório")]
        public decimal Preco { get; set; }

        public int Quantidade { get; set; }

        // Permitindo que o caminho da imagem seja nulo (string?)
        [StringLength(500)] // Boa prática
        public string? CaminhoImagem { get; set; }
    }
}